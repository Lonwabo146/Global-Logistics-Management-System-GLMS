using GLMS.Models;

namespace GLMS.Services.Factories
{
    public abstract class ContractFactory
    {
        public abstract Contract CreateContract(
            string serviceLevel,
            DateTime startDate, 
            DateTime endDate, 
            int customerId);
    }
}
