using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

public class PositionsControllerTests
{
    // Helper method that creates a controller instance for testing
    // It also sets up a fake configuration and logger
    private PositionsController CreateController()
    {
        // Fake configuration so the controller can read a connection string
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", "Server=localhost\\SQLEXPRESS;Database=InfralabsDB;Trusted_Connection=True;TrustServerCertificate=True;" }
        };

        // Build configuration from the in-memory dictionary
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Create a simple logger for the controller
        ILogger<PositionsController> logger = new LoggerFactory()
            .CreateLogger<PositionsController>();

        // Return a controller instance with the dependencies
        return new PositionsController(configuration, logger);
    }

    [Fact]
    public void PostPositions_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        // Create the controller and a position with an empty name
        var controller = CreateController();

        var position = new Position
        {
            Name = "",
            Lat = 37.98,
            Lon = 23.72
        };

        // Act
        // Call the method we want to test
        IActionResult result = controller.PostPositions(position);

        // Assert
        // We expect a BadRequest response
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid input data", badRequest.Value);
    }

    [Fact]
    public void PostPositions_NullName_ReturnsBadRequest()
    {
        // Arrange
        // Create controller and position with null name
        var controller = CreateController();

        var position = new Position
        {
            Name = null,
            Lat = 37.98,
            Lon = 23.72
        };

        // Act
        IActionResult result = controller.PostPositions(position);

        // Assert
        // Should return BadRequest because name is null
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid input data", badRequest.Value);
    }

    [Fact]
    public void PostPositions_WhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        // Create controller and position with only spaces as name
        var controller = CreateController();

        var position = new Position
        {
            Name = "   ",
            Lat = 37.98,
            Lon = 23.72
        };

        // Act
        IActionResult result = controller.PostPositions(position);

        // Assert
        // Should return BadRequest because whitespace is not a valid name
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid input data", badRequest.Value);
    }
}