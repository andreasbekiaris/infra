using System;
using Xunit;
using Microsoft.AspNetCore.Mvc;

public class ErrorHandlingTests
{
    [Fact]
    public void GetPositions_DatabaseFailure_Returns500()
    {
        // Arrange
        // Χαλάμε επίτηδες το connection environment
        Environment.SetEnvironmentVariable("DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER", "0");

        var controller = new PositionsController();

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
    var controller = new PositionsController();

    var position = new Position
    {
        Name = "Athens",
        Lat = 37.98,
        Lon = 23.72
    };

    // Προκαλούμε SQL failure κλείνοντας το SQL service ή χαλώντας connection env
    Environment.SetEnvironmentVariable("DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER", "0");

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
    var controller = new PositionsController();

    Environment.SetEnvironmentVariable("DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER", "0");

    // Act
    var result = controller.Getposdistance("Athens");

    // Assert
    var objectResult = Assert.IsType<ObjectResult>(result.Result);
    Assert.Equal(500, objectResult.StatusCode);
    Assert.Equal("Internal server error", objectResult.Value);
}
}