using System;
using System.Collections.Generic;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class RebateServiceTests
{
    private readonly FakeRebateDataStore rebateStore = new();
    private readonly FakeProductDataStore productStore = new();
    private readonly StubCalculator calculator = new(IncentiveType.FixedCashAmount);

    private readonly CalculateRebateRequest request = new()
    {
        RebateIdentifier = "rebate-1",
        ProductIdentifier = "product-1",
        Volume = 3m
    };

    private RebateService CreateService(params IRebateCalculator[] calculators)
    {
        return new RebateService(rebateStore, productStore, calculators);
    }

    [Fact]
    public void Calculate_ValidRequest_UsesMatchingCalculatorAndSavesResult()
    {
        var otherCalculator = new StubCalculator(IncentiveType.AmountPerUom);
        var service = CreateService(otherCalculator, calculator);

        var result = service.Calculate(request);

        Assert.True(result.Success);
        Assert.Equal(0, otherCalculator.CallCount);
        Assert.Equal(1, calculator.CallCount);

        var saved = Assert.Single(rebateStore.SavedCalculations);
        Assert.Equal(calculator.Amount, saved.Amount);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Calculate_MissingRecord_FailsWithoutCalculatingOrSaving(
        bool missingRebate,
        bool missingProduct)
    {
        if (missingRebate)
        {
            rebateStore.Rebate = null;
        }

        if (missingProduct)
        {
            productStore.Product = null;
        }

        var service = CreateService(calculator);

        var result = service.Calculate(request);

        Assert.False(result.Success);
        Assert.Equal(0, calculator.CallCount);
        Assert.Empty(rebateStore.SavedCalculations);
    }

    [Fact]
    public void Calculate_NoMatchingCalculator_FailsWithoutSaving()
    {
        rebateStore.Rebate.Incentive = IncentiveType.AmountPerUom;
        var service = CreateService(calculator);

        var result = service.Calculate(request);

        Assert.False(result.Success);
        Assert.Equal(0, calculator.CallCount);
        Assert.Empty(rebateStore.SavedCalculations);
    }

    [Fact]
    public void Calculate_RejectedCalculation_DoesNotSave()
    {
        calculator.Success = false;
        var service = CreateService(calculator);

        var result = service.Calculate(request);

        Assert.False(result.Success);
        Assert.Equal(1, calculator.CallCount);
        Assert.Empty(rebateStore.SavedCalculations);
    }

    [Fact]
    public void Calculate_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateService(calculator);

        Assert.Throws<ArgumentNullException>(
            () => service.Calculate(null));
    }


    // test calculator that lets us control whether calculation succeeds
    private sealed class StubCalculator : IRebateCalculator
    {
        public StubCalculator(IncentiveType incentiveType)
        {
            IncentiveType = incentiveType;
        }

        public IncentiveType IncentiveType { get; }

        public bool Success { get; set; } = true;

        public decimal Amount { get; } = 42.125m;

        public int CallCount { get; private set; }

        public bool TryCalculate(
            Rebate rebate,
            Product product,
            CalculateRebateRequest request,
            out decimal amount)
        {
            CallCount++;

            if (Success)
            {
                amount = Amount;
                return true;
            }

            amount = 0m;
            return false;
        }
    }


    // fake store so tests do not access a real database
    private sealed class FakeRebateDataStore : IRebateDataStore
    {
        public Rebate Rebate { get; set; } = new()
        {
            Identifier = "rebate-1",
            Incentive = IncentiveType.FixedCashAmount
        };

        public List<(Rebate Rebate, decimal Amount)> SavedCalculations { get; }
            = new();

        public Rebate GetRebate(string rebateIdentifier)
        {
            return Rebate;
        }

        public void StoreCalculationResult(
            Rebate rebate,
            decimal rebateAmount)
        {
            SavedCalculations.Add((rebate, rebateAmount));
        }
    }


    private sealed class FakeProductDataStore : IProductDataStore
    {
        public Product Product { get; set; } = new()
        {
            Identifier = "product-1"
        };

        public Product GetProduct(string productIdentifier)
        {
            return Product;
        }
    }
}