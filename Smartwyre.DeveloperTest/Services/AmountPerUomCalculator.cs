using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;

public class AmountPerUomCalculator : IRebateCalculator
{
    public IncentiveType IncentiveType => IncentiveType.AmountPerUom;

    public bool TryCalculate(Rebate rebate, Product product, CalculateRebateRequest request, out decimal amount)
    {
        amount = 0m;
        if (!product.SupportedIncentives.HasFlag(SupportedIncentiveType.AmountPerUom) ||
            rebate.Amount == 0 ||
            request.Volume == 0)
        {
            return false;
        }
        amount = rebate.Amount * request.Volume;
        return true;
    }
}
