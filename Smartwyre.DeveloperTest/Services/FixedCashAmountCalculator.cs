using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;

public class FixedCashAmountCalculator : IRebateCalculator
{
    public IncentiveType IncentiveType => IncentiveType.FixedCashAmount;

    public bool TryCalculate(Rebate rebate, Product product, CalculateRebateRequest request, out decimal amount)
    {
        amount = 0m;
        if (!product.SupportedIncentives.HasFlag(SupportedIncentiveType.FixedCashAmount) ||
            rebate.Amount == 0)
        {
            return false;
        }

        amount = rebate.Amount;
        return true;
    }
}
