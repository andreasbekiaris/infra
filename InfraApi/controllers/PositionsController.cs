using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    // Connection string that we read from appsettings.json
    private readonly string _connectionString;

    // Logger so we can see what happens in the API
    private readonly ILogger<PositionsController> _logger;

    // Constructor – ASP.NET injects configuration and logger here
    public PositionsController(IConfiguration configuration, ILogger<PositionsController> logger)
    {
        // Get the connection string from configuration
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");

        _logger = logger;
    }

    // GET /api/positions
    // Returns all positions stored in the database
    [HttpGet]
    public IActionResult GetPositions()
    {
        try
        {
            // List where we will store the results
            List<Position> positions = new List<Position>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Query that selects all positions
                SqlCommand cmd = new SqlCommand(
                    "SELECT Pos_name, pos_lat, pos_lon FROM positions ORDER BY pos_name ASC",
                    con
                );

                SqlDataReader reader = cmd.ExecuteReader();

                // Read each row from the database
                while (reader.Read())
                {
                    // Create a Position object from the row
                    Position p = new Position
                    {
                        Name = reader.GetString(0),
                        Lat = reader.GetDouble(1),
                        Lon = reader.GetDouble(2)
                    };

                    positions.Add(p);
                }
            }

            // Just logging how many we returned
            _logger.LogInformation("Retrieved {Count} positions.", positions.Count);

            return Ok(positions);
        }
        catch (Exception ex)
        {
            // If something goes wrong (DB connection etc)
            _logger.LogError(ex, "Error while getting positions.");

            return StatusCode(500, "Internal server error");
        }
    }

    // POST /api/positions
    // Adds a new position to the database
    [HttpPost]
    public IActionResult PostPositions(Position b)
    {
        try
        {
            // Check if body is empty
            if (b == null)
            {
                return BadRequest("Invalid input data");
            }

            // Check that name exists
            if (string.IsNullOrWhiteSpace(b.Name))
            {
                return BadRequest("Invalid input data");
            }

            // Validate coordinates
            if (b.Lat < -90 || b.Lat > 90 || b.Lon < -180 || b.Lon > 180)
            {
                return BadRequest("Invalid input data");
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // First check if the name already exists
                string query = "SELECT COUNT(*) FROM positions WHERE Pos_name = @Name";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Name", b.Name);

                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    // Name already exists
                    return Conflict("Position name already exists.");
                }

                // Insert the new position
                string query1 = "INSERT INTO positions (Pos_name, pos_lat, pos_lon) VALUES (@Name, @Lat, @Lon)";
                SqlCommand cmd1 = new SqlCommand(query1, con);
                cmd1.Parameters.AddWithValue("@Name", b.Name);
                cmd1.Parameters.AddWithValue("@Lat", b.Lat);
                cmd1.Parameters.AddWithValue("@Lon", b.Lon);

                cmd1.ExecuteNonQuery();
            }

            _logger.LogInformation("Inserted position {Name}.", b.Name);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while inserting position.");

            return StatusCode(500, "Internal server error");
        }
    }

    // GET /api/positions/{name}/distance
    // Calculates distance from this position to all the others
    [HttpGet("{name}/distance")]
    public ActionResult<List<Distance>> Getposdistance(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Invalid name");
            }

            double targetLat;
            double targetLon;

            List<Position> positions = new List<Position>();
            List<Distance> distances = new List<Distance>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // First we get the coordinates of the requested position
                string query = "SELECT Pos_name, pos_lat, pos_lon FROM positions WHERE Pos_name = @Name";
                SqlCommand cmd1 = new SqlCommand(query, con);
                cmd1.Parameters.AddWithValue("@Name", name);

                SqlDataReader reader1 = cmd1.ExecuteReader();

                if (reader1.Read())
                {
                    targetLat = reader1.GetDouble(1);
                    targetLon = reader1.GetDouble(2);
                }
                else
                {
                    // Position not found
                    return NotFound();
                }

                reader1.Close();

                // Now get all positions
                SqlCommand cmd = new SqlCommand(
                    "SELECT Pos_name, pos_lat, pos_lon FROM positions ORDER BY pos_name ASC",
                    con
                );

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Position p = new Position
                    {
                        Name = reader.GetString(0),
                        Lat = reader.GetDouble(1),
                        Lon = reader.GetDouble(2)
                    };

                    positions.Add(p);
                }

                // Calculate distance for each position
                foreach (var p in positions)
                {
                    if (p.Name == name)
                        continue;

                    double km = DistanceCalculator.CalculateDistance(
                        targetLat,
                        targetLon,
                        p.Lat,
                        p.Lon
                    );

                    distances.Add(new Distance
                    {
                        Name = p.Name,
                        Distance_km = km
                    });
                }
            }

            _logger.LogInformation("Calculated distances from {Name}.", name);

            return Ok(distances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while calculating distances.");

            return StatusCode(500, "Internal server error");
        }
    }
}