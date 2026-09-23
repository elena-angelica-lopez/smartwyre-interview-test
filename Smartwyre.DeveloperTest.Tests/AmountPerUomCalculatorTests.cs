using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class AmountPerUomCalculatorTests
{
    // happy path
    [Fact]
    public void TryCalculate_ValidRebate_ReturnsCorrectAmount()
    {
        var calculator = new AmountPerUomCalculator();
        var rebate = new Rebate { Amount = 5m };
        var product = new Product
        {
            SupportedIncentives = SupportedIncentiveType.AmountPerUom
        };
        var request = new CalculateRebateRequest { Volume = 3m };

        var success = calculator.TryCalculate(
            rebate, product, request, out var amount);

        Assert.True(success);
        Assert.Equal(15m, amount);
    }

    // incorrect data 
    [Theory]
    [InlineData(0, 3)]
    [InlineData(5, 0)]
    public void TryCalculate_ZeroRequiredValue_ReturnsFalse(
        decimal rebateAmount,
        decimal volume)
    {
        var calculator = new AmountPerUomCalculator();
        var rebate = new Rebate { Amount = rebateAmount };
        var product = new Product
        {
            SupportedIncentives = SupportedIncentiveType.AmountPerUom
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
        var calculator = new AmountPerUomCalculator();
        var rebate = new Rebate { Amount = 5m };
        var product = new Product
        {
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount
        };
        var request = new CalculateRebateRequest { Volume = 3m };

        var success = calculator.TryCalculate(
            rebate, product, request, out var amount);

        Assert.False(success);
        Assert.Equal(0m, amount);
    }
}
