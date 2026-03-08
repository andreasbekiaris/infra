using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{


    [HttpGet]
public IActionResult GetPositions()
{
   try {
List<Position> positions = new List<Position>();
string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=InfralabsDB;Trusted_Connection=True;TrustServerCertificate=True;";
    using (SqlConnection con = new SqlConnection(ConnectionString)){
        con.Open();
        SqlCommand cmd = new SqlCommand("SELECT Pos_name,lat,lon FROM positions ORDER BY pos_name ASC",con);

        SqlDataReader reader = cmd.ExecuteReader();
        while(reader.Read()){
Position p  = new Position {
   
    Name =reader.GetString(0),
Lat= reader.GetDouble(1),
Lon= reader.GetDouble(2)

};
positions.Add(p);
        }
    }
return Ok(positions);

}
        catch
        {
            return StatusCode(500,"Internal server error");
        }
}


[HttpPost]
public IActionResult PostPositions(Position b)
    {
    try {
    if (!string.IsNullOrWhiteSpace(b.Name))
    {
        string ConnectionString1 = "Server=localhost\\SQLEXPRESS;Database=InfralabsDB;Trusted_Connection=True;TrustServerCertificate=True;";
         using (SqlConnection con = new SqlConnection(ConnectionString1)){
        con.Open();
        string  query = "SELECT COUNT(*) FROM positions WHERE Pos_name = @Name";
        SqlCommand cmd = new SqlCommand(query,con);
        cmd.Parameters.AddWithValue("@Name",b.Name);
        int count = (int)cmd.ExecuteScalar();
        if (count ==  0)
            {
                if((b.Lat>=-90 && b.Lat<=90) && (b.Lon<=180 && b.Lon>=-180 )){
                string query1 = "INSERT INTO positions (Pos_name,Lat,lon)  VALUES (@Name , @Lat,@Lon)";
                SqlCommand cmd1 = new SqlCommand(query1,con);
                cmd1.Parameters.AddWithValue("@Name",b.Name);
                cmd1.Parameters.AddWithValue("@Lat",b.Lat);
                cmd1.Parameters.AddWithValue("@Lon",b.Lon);
                cmd1.ExecuteNonQuery();
                return Ok();
                }
                else
                {
                   
                    return BadRequest("Invalid input data");
                }
            }
            else
            {
               
                return Conflict();
            }
    }
    
} 
else
    {
        
        return BadRequest("Invalid input data");
    }
    }
        catch
        {
            return StatusCode(500,"Internal server error");
        }
 }


[HttpGet("{name}/distance")]
public ActionResult<List<Distance>> Getposdistance(string name)
    {
        try{
      double targetLat ;
      double targetLon ;
 List<Position> positions = new List<Position>();
 List<Distance> distances = new List<Distance>();
string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=InfralabsDB;Trusted_Connection=True;TrustServerCertificate=True;";
    using (SqlConnection con = new SqlConnection(ConnectionString)){
        con.Open();
        SqlCommand cmd = new SqlCommand("SELECT Pos_name,lat,lon FROM positions ORDER BY pos_name ASC",con);
string  query = "SELECT Pos_name, Lat, Lon FROM positions WHERE Pos_name = @Name";
        SqlCommand cmd1 = new SqlCommand(query,con);
        cmd1.Parameters.AddWithValue("@Name",name);
       SqlDataReader reader1 = cmd1.ExecuteReader();
        
        if (reader1.Read())
{
      targetLat = reader1.GetDouble(1);
     targetLon = reader1.GetDouble(2);
}
else
            {
                return NotFound();
            }
reader1.Close();

        SqlDataReader reader = cmd.ExecuteReader();
        while(reader.Read()){
            
Position p  = new Position {
   
    Name =reader.GetString(0),
Lat= reader.GetDouble(1),
Lon= reader.GetDouble(2)

};
positions.Add(p);
    }
for(int i = 0; i < positions.Count; i++)
{
    Position p = positions[i];

    if (p.Name == name)
    {
        continue;
    }

    double km = DistanceCalculator.CalculateDistance( targetLat,  targetLon,  p.Lat , p.Lon );

    Distance b = new Distance
    {
        Name = p.Name,
        Distance_km = km
    };

    distances.Add(b);
}
          
}
return Ok(distances);
    }
    catch
        {
            return StatusCode(500,"Internal server error");
        }
    {
        
    }
    }
}