using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class FixedRateRebateCalculatorTests
{
    // happy path
    [Fact]
    public void TryCalculate_ValidRebate_ReturnsCorrectAmount()
    {
        var calculator = new FixedRateRebateCalculator();
        var rebate = new Rebate { Percentage = 0.1m };
        var product = new Product
        {
            Price = 100m,
            SupportedIncentives = SupportedIncentiveType.FixedRateRebate
        };
        var request = new CalculateRebateRequest { Volume = 3m };

        var success = calculator.TryCalculate(
            rebate, product, request, out var amount);

        Assert.True(success);
        Assert.Equal(30m, amount);
    }

    // incorrect data 
    [Theory]
    [InlineData(0, 1, 3)]
    [InlineData(100, 0, 3)]
    [InlineData(100, 1, 0)]
    public void TryCalculate_ZeroRequiredValue_ReturnsFalse(
        decimal price,
        decimal percentage,
        decimal volume)
    {
        var calculator = new FixedRateRebateCalculator();
        var rebate = new Rebate { Percentage = percentage };
        var product = new Product
        {
            Price = price,
            SupportedIncentives = SupportedIncentiveType.FixedRateRebate
        };
        var request = new CalculateRebateRequest { Volume = volume };

        var success = calculator.TryCalculate(
            rebate, product, request, out var amount);

        Assert.False(success);
        Assert.Equal(0m, amount);
    }

    // unsupported incentive
    [Fact]
    public void TryCalculate_UnsupportedIncentive_ReturnsFalse()
    {
        var calculator = new FixedRateRebateCalculator();
        var rebate = new Rebate { Percentage = 0.1m };
        var product = new Product
        {
            Price = 100m,
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount
        };
        var request = new CalculateRebateRequest { Volume = 3m };

        var success = calculator.TryCalculate(
            rebate, product, request, out var amount);

        Assert.False(success);
        Assert.Equal(0m, amount);
    }

}