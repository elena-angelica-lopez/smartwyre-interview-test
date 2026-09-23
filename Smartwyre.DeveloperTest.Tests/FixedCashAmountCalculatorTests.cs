using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class FixedCashAmountCalculatorTests
{
    // happy path
    [Fact]
    public void TryCalculate_ValidRebate_ReturnsCorrectAmount()
    {
        var calculator = new FixedCashAmountCalculator();
        var rebate = new Rebate { Amount = 25m };
        var product = new Product
        {
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount
        };
        var request = new CalculateRebateRequest();

        var success = calculator.TryCalculate(
            rebate, product, request, out var amount);

        Assert.True(success);
        Assert.Equal(25m, amount);
    }

    // incorrect data 
    [Fact]
    public void TryCalculate_ZeroAmount_ReturnsFalse()
    {
        var calculator = new FixedCashAmountCalculator();
        var rebate = new Rebate { Amount = 0m };
        var product = new Product
        {
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount
        };
        var request = new CalculateRebateRequest();

        var success = calculator.TryCalculate(
            rebate, product, request, out var amount);

        Assert.False(success);
        Assert.Equal(0m, amount);
    }

    // unsupported incentive
    [Fact]
    public void TryCalculate_UnsupportedIncentive_ReturnsFalse()
    {
        var calculator = new FixedCashAmountCalculator();
        var rebate = new Rebate { Amount = 25m };
        var product = new Product
        {
            SupportedIncentives = SupportedIncentiveType.FixedRateRebate
        };
        var request = new CalculateRebateRequest();

        var success = calculator.TryCalculate(
            rebate, product, request, out var amount);

        Assert.False(success);
        Assert.Equal(0m, amount);
    }
}
