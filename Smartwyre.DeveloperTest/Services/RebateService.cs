using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Smartwyre.DeveloperTest.Services;

public class RebateService : IRebateService
{
    private readonly IRebateDataStore rebateDataStore;
    private readonly IProductDataStore productDataStore;
    private readonly IEnumerable<IRebateCalculator> calculators;

    // refactor for DI: accept interfaces to use real stores OR test/mock stores
    public RebateService(IRebateDataStore rebateDataStore, IProductDataStore productDataStore,
        IEnumerable<IRebateCalculator> calculators)
    {
        //throw for missing dependencies
        if (rebateDataStore == null)
        {
            throw new ArgumentNullException(nameof(rebateDataStore));
        }

        if (productDataStore == null)
        {
            throw new ArgumentNullException(nameof(productDataStore));
        }

        if (calculators == null)
        {
            throw new ArgumentNullException(nameof(calculators));
        }

        this.rebateDataStore = rebateDataStore;
        this.productDataStore = productDataStore;
        this.calculators = calculators;
    }

    public CalculateRebateResult Calculate(CalculateRebateRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        Rebate rebate = rebateDataStore.GetRebate(request.RebateIdentifier);
        Product product = productDataStore.GetProduct(request.ProductIdentifier);

        var result = new CalculateRebateResult();

        // calculators receive existing records and only check incentive-specific rules
        if (rebate == null || product == null)
        {
            return result;
        }

        var calculator = calculators.FirstOrDefault(c => c.IncentiveType == rebate.Incentive);
        if (calculator == null)
        {
            return result;
        }

        result.Success = calculator.TryCalculate(rebate, product, request, out var rebateAmount);

        if (result.Success)
        {
            // reuse the injected store for both lookup and persistence
            rebateDataStore.StoreCalculationResult(rebate, rebateAmount);
        }

        return result;
    }
}
