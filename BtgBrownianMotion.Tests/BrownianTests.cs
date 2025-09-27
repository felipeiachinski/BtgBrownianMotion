using BtgBrownianMotion.Core.Models;
using Xunit;

namespace BtgBrownianMotion.Tests;

public class BrownianTests
{
    [Fact]
    public void Constant_WhenSigmaAndMeanZero()
    {
        var prices = Brownian.GenerateBrownianMotion(0, 0, 100, 10);
        Assert.Equal(10, prices.Length);
        Assert.All(prices, p => Assert.Equal(100, p, 5));
    }

    [Fact]
    public void LengthAndFirstPrice_AreCorrect()
    {
        var prices = Brownian.GenerateBrownianMotion(0.01, 0.0, 123.45, 252);
        Assert.Equal(252, prices.Length);
        Assert.Equal(123.45, prices[0], 5);
    }
}

