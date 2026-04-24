using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    public async Task< IActionResult> GetPositionsAsync()
    {
        try
        {
            // List where we will store the results
            List<Position> positions = new List<Position>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // Query that selects all positions
               using  SqlCommand cmd = new SqlCommand("SELECT pos_name, pos_lat, pos_lon FROM positions ORDER BY pos_name ASC", con );

        using  SqlDataReader reader = await cmd.ExecuteReaderAsync();

                // Read each row from the database
                while (await reader.ReadAsync())
                {
                    // Create a Position object from the row
                    Position p = new Position
                    {
                        Name =reader.GetString(reader.GetOrdinal("pos_name")),
                        Lat = reader.GetDouble(reader.GetOrdinal("pos_lat")),
                        Lon = reader.GetDouble(reader.GetOrdinal("pos_lon"))
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
    public async Task<IActionResult> PostPositions(Position b)
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
                await con.OpenAsync();

                // First check if the name already exists
             string query = @"INSERT INTO positions (pos_name, pos_lat, pos_lon) SELECT @Name, @Lat, @Lon WHERE NOT EXISTS (SELECT 1 FROM positions WHERE pos_name = @Name)";

        using SqlCommand cmd = new SqlCommand(query, con);                            
             cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = b.Name;
cmd.Parameters.Add("@Lat", SqlDbType.Float).Value = b.Lat;
cmd.Parameters.Add("@Lon", SqlDbType.Float).Value = b.Lon;

              int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0  )

                {
                    // Name already exists
                    return Conflict("Position name already exists.");
                }        
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
public async Task<ActionResult<List<Distance>>> Getposdistance(string name)
{
    try
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Invalid name");

        var distances = new List<Distance>();

        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            await con.OpenAsync();

            // 1️⃣ Check if the name exists
            string existsQuery = "SELECT COUNT(1) FROM positions WHERE pos_name = @Name";
          using  SqlCommand existsCmd = new SqlCommand(existsQuery, con);
            existsCmd.Parameters.AddWithValue("@Name", name);

            int count = (int)(await existsCmd.ExecuteScalarAsync() ?? 0);

            if (count == 0)
                return NotFound();

            // 2️⃣ One query to get everything
string query = @"
    SELECT 
        other.pos_name,
        other.pos_lat,
        other.pos_lon,
        target.pos_lat AS target_lat,
        target.pos_lon AS target_lon
    FROM positions other
    CROSS JOIN positions target
    WHERE target.pos_name = @Name
      AND other.pos_name != @Name
    ORDER BY other.pos_name ASC";

        using    SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Name", name);

            // 3️⃣ One reader with column names
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    double km = DistanceCalculator.CalculateDistance(
                        reader.GetDouble(reader.GetOrdinal("target_lat")),
                        reader.GetDouble(reader.GetOrdinal("target_lon")),
                        reader.GetDouble(reader.GetOrdinal("pos_lat")),
                        reader.GetDouble(reader.GetOrdinal("pos_lon"))
                    );

                    distances.Add(new Distance
                    {
                        Name = reader.GetString(reader.GetOrdinal("pos_name")),
                        Distance_km = km
                    });
                }
            }
        }

        _logger.LogInformation("Calculated distances from {Name}.", name);
        return Ok(distances);
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Database error while calculating distances from {Name}.", name);
        return StatusCode(500, "Database error");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error while calculating distances from {Name}.", name);
        return StatusCode(500, "Internal server error");
    }
}
}