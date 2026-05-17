namespace GLMS.Models
{
    public interface ISubject
    {
        void RegisterObserver(IContractObserver observer);
        void RemoveObserver(IContractObserver observer);
        Task NotifyObservers(Contract contract, string changeDescription);
    }
}
