using System.Collections.Generic;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class ErrorHandlingTests
{
    private PositionsController CreateBrokenController()
    {
        var settings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", "Server=INVALIDSERVER;Database=FakeDB;" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        ILogger<PositionsController> logger = new LoggerFactory()
            .CreateLogger<PositionsController>();

        return new PositionsController(configuration, logger);
    }

    [Fact]
    public async Task GetPositions_DatabaseFailure_Returns500()
    {
        // Arrange
      var controller = CreateBrokenController();

        // Act
        IActionResult result = await controller.GetPositions();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Internal server error", objectResult.Value);
    }

    [Fact]
    public async Task  PostPositions_DatabaseFailure_Returns500()
    {
        // Arrange
        var controller = CreateBrokenController();

        var position = new Position
        {
            Name = "Athens",
            Lat = 37.98,
            Lon = 23.72
        };

        // Act
        IActionResult result = await controller.PostPositions(position);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Internal server error", objectResult.Value);
    }

    [Fact]
    public async Task  Getposdistance_DatabaseFailure_Returns500()
    {
        // Arrange
        var controller = CreateBrokenController();

        // Act
        var result = await controller.Getposdistance("Athens");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Database error", objectResult.Value);
    }
}