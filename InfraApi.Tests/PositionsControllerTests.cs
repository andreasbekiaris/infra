using Xunit;
using Microsoft.AspNetCore.Mvc;


{
    
}
public class PositionsControllerTests
{
    [Fact]
    public void PostPositions_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var controller = new PositionsController();

        var position = new Position
        {
            Name = "",
            Lat = 37.98,
            Lon = 23.72
        };

        // Act
        IActionResult result = controller.PostPositions(position);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid input data", badRequest.Value);
    }

    [Fact]
    public void PostPositions_NullName_ReturnsBadRequest()
    {
        // Arrange
        var controller = new PositionsController();

        var position = new Position
        {
            Name = null,
            Lat = 37.98,
            Lon = 23.72
        };

        // Act
        IActionResult result = controller.PostPositions(position);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid input data", badRequest.Value);
    }

    [Fact]
    public void PostPositions_WhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        var controller = new PositionsController();

        var position = new Position
        {
            Name = "   ",
            Lat = 37.98,
            Lon = 23.72
        };

        // Act
        IActionResult result = controller.PostPositions(position);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid input data", badRequest.Value);
    }
}