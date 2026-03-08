using System ;
namespace InfraApi; 


public class GeoHelperTests
{
    [Fact]
    public void Distance_SamePoint_ReturnsZero()
    {
        double result = DistanceCalculator.CalculateDistance(37,23,37,23);

        Assert.Equal(0, result, 5);
    }
}
