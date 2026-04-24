using System ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace InfraApi.Tests;

public class GeoHelperTests
{
    
    // 1. Same point → zero distance
    
    [Fact]
    public void Distance_SamePoint_ReturnsZero()
    {
        double result = DistanceCalculator.CalculateDistance(37, 23, 37, 23);

        Assert.Equal(0, result, precision: 5);
    }

   
    // 2. Known real-world distance: Athens → London ≈ 2395 km

    [Fact]
    public void Distance_AthensToLondon_ReturnsApprox2395km()
    {
        // Athens:  37.98 N, 23.72 E
        // London:  51.50 N,  0.12 W
        double result = DistanceCalculator.CalculateDistance(37.98, 23.72, 51.50, -0.12);

        // Allow ±15 km tolerance for floating-point and formula variance
        Assert.InRange(result, 2380, 2410);
    }

   
    // 3. Antipodal points (directly opposite sides of the Earth)
    //    Maximum possible great-circle distance ≈ 20,015 km
    //    (0,0) and (0,180) are perfect antipodes
   
    [Fact]
    public void Distance_AntipodalPoints_ReturnsMaxDistance()
    {
        double result = DistanceCalculator.CalculateDistance(0, 0, 0, 180);

        // Half of Earth's circumference ≈ 20,015 km  (±20 km tolerance)
        Assert.InRange(result, 19_995, 20_035);
    }

    
    // 4. Crossing the International Date Line: lon +179 → lon -179
    //    These two points are only ~222 km apart near the equator,
    //    NOT ~19,000 km — a naive implementation will get this wrong.
    
    [Fact]
    public void Distance_CrossingDateLine_ReturnsShortDistance()
    {
        // Both points on the equator, 2° apart across the date line
        double result = DistanceCalculator.CalculateDistance(0, 179, 0, -179);

        // 2° of longitude at the equator ≈ 222 km  (±5 km tolerance)
        Assert.InRange(result, 217, 227);
    }

    // 5a. North Pole to South Pole ≈ 20,015 km (same as antipodal)
    
    [Fact]
    public void Distance_NorthPoleToSouthPole_ReturnsHalfCircumference()
    {
        double result = DistanceCalculator.CalculateDistance(90, 0, -90, 0);

        Assert.InRange(result, 19_995, 20_035);
    }

    
    // 5b. Any point to the North Pole — longitude is irrelevant at the poles
    //     Athens (37.98, 23.72) → North Pole (90, 0) ≈ 5,793 km
    //     Changing the pole's longitude must NOT change the result.
    
    [Fact]
    public void Distance_ToNorthPole_LongitudeOfPoleIsIrrelevant()
    {
        double resultLon0   = DistanceCalculator.CalculateDistance(37.98, 23.72, 90,  0);
        double resultLon90  = DistanceCalculator.CalculateDistance(37.98, 23.72, 90, 90);
        double resultLon180 = DistanceCalculator.CalculateDistance(37.98, 23.72, 90, 180);

        // All three calls must produce the same distance (±1 m tolerance)
        Assert.Equal(resultLon0, resultLon90,  precision: 0);
        Assert.Equal(resultLon0, resultLon180, precision: 0);

        // And the distance itself should be roughly 5,793 km
        Assert.InRange(resultLon0, 5_780, 5_810);
    }

    
    // 6. Symmetry: dist(A → B) must equal dist(B → A)
    //    Tested with three different pairs to be thorough.
   
    [Theory]
    [InlineData(37.98,  23.72, 51.50,  -0.12)]   // Athens  → London
    [InlineData(40.71, -74.00, 35.68, 139.69)]   // New York → Tokyo
    [InlineData(0,      179,   0,     -179  )]   // Date line crossing
    public void Distance_IsSymmetric(double lat1, double lon1, double lat2, double lon2)
    {
        double ab = DistanceCalculator.CalculateDistance(lat1, lon1, lat2, lon2);
        double ba = DistanceCalculator.CalculateDistance(lat2, lon2, lat1, lon1);

        Assert.Equal(ab, ba, precision: 5);
    }
}
