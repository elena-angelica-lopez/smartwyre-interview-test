using System;

using System.Globalization;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Runner;

class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("1: Fixed cash amount");
        Console.WriteLine("2: Fixed rate rebate");
        Console.WriteLine("3: Amount per unit");
        Console.Write("Choose an incentive (1-3): ");

        var rebate = new Rebate { Identifier = "input-rebate" };
        var product = new Product { Identifier = "input-product" };
        var request = new CalculateRebateRequest
        {
            RebateIdentifier = rebate.Identifier,
            ProductIdentifier = product.Identifier
        };

        try
        {
            switch (Console.ReadLine())
            {
                case "1":
                    rebate.Incentive = IncentiveType.FixedCashAmount;
                    product.SupportedIncentives = SupportedIncentiveType.FixedCashAmount;
                    rebate.Amount = ReadDecimal("Rebate amount: ");
                    break;
                case "2":
                    rebate.Incentive = IncentiveType.FixedRateRebate;
                    product.SupportedIncentives = SupportedIncentiveType.FixedRateRebate;
                    product.Price = ReadDecimal("Product price: ");
                    rebate.Percentage = ReadDecimal("Rebate rate (e.g. 0.1 for 10%): ");
                    request.Volume = ReadDecimal("Volume: ");
                    break;
                case "3":
                    rebate.Incentive = IncentiveType.AmountPerUom;
                    product.SupportedIncentives = SupportedIncentiveType.AmountPerUom;
                    rebate.Amount = ReadDecimal("Rebate amount per unit: ");
                    request.Volume = ReadDecimal("Volume: ");
                    break;
                default:
                    Console.WriteLine("Please run again and choose 1, 2, or 3.");
                    return 1;
            }
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid number. Use a dot for decimals (e.g. 2.5).");
            return 1;
        }

        var service = new RebateService(
            new ConsoleRebateDataStore(rebate),
            new ConsoleProductDataStore(product),
            new IRebateCalculator[]
            {
                new FixedCashAmountCalculator(),
                new FixedRateRebateCalculator(),
                new AmountPerUomCalculator()
            });

        var result = service.Calculate(request);
        if (!result.Success)
        {
            Console.WriteLine("Calculation failed: required values must be nonzero.");
            return 1;
        }

        return 0;
    }

    private static decimal ReadDecimal(string prompt)
    {
        Console.Write(prompt);
        if (!decimal.TryParse(Console.ReadLine(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException();
        }
        return value;
    }

    private sealed class ConsoleRebateDataStore : IRebateDataStore
    {
        private readonly Rebate rebate;

        public ConsoleRebateDataStore(Rebate rebate) => this.rebate = rebate;

        public Rebate GetRebate(string rebateIdentifier) =>
            rebateIdentifier == rebate.Identifier ? rebate : null;

        public void StoreCalculationResult(Rebate rebate, decimal rebateAmount)
        {
            Console.WriteLine($"Rebate amount: {rebateAmount.ToString(CultureInfo.InvariantCulture)}");
            Console.WriteLine("Result displayed only; no database storage.");
        }
    }

    private sealed class ConsoleProductDataStore : IProductDataStore
    {
        private readonly Product product;

        public ConsoleProductDataStore(Product product) => this.product = product;

        public Product GetProduct(string productIdentifier) =>
            productIdentifier == product.Identifier ? product : null;
    }
}
