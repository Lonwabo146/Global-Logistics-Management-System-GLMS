namespace GLMS.Models
{
    public interface IContractObserver
    {
        Task Update(Contract contract, string changeDescription);
    }
}
