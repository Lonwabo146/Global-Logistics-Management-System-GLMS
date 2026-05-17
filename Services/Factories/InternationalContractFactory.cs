using GLMS.Models;

namespace GLMS.Services.Factories
{
    public class InternationalContractFactory : ContractFactory
    {
        public override Contract CreateContract(
            string serviceLevel,
            DateTime startDate,
            DateTime endDate,
            int customerId)
        {
            return new Contract
            {
                ServiceLevel = $"[INTL] {serviceLevel}",
                StartDate = startDate,
                EndDate = endDate,
                CustomerId = customerId,
                Status = ContractStatus.Active
            };
        }
    }
}