using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;

public interface IRebateCalculator
{
    IncentiveType IncentiveType { get; }

    // return eligibility and calculated amount, shared shape
    // caller must already verify a non-null rebate, product, and request
    bool TryCalculate(Rebate rebate, Product product, CalculateRebateRequest request, out decimal amount);
}
