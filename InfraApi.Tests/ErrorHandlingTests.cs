using System.Collections.Generic;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    public void GetPositions_DatabaseFailure_Returns500()
    {
        // Arrange
        var controller = CreateBrokenController();

        // Act
        IActionResult result = controller.GetPositions();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Internal server error", objectResult.Value);
    }

    [Fact]
    public void PostPositions_DatabaseFailure_Returns500()
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
        IActionResult result = controller.PostPositions(position);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Internal server error", objectResult.Value);
    }

    [Fact]
    public void Getposdistance_DatabaseFailure_Returns500()
    {
        // Arrange
        var controller = CreateBrokenController();

        // Act
        var result = controller.Getposdistance("Athens");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Internal server error", objectResult.Value);
    }
}