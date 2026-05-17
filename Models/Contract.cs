using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Models
{
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }

    public class Contract : ISubject
    {
        public int ContractId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; }

        [Required]
        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; }

        [Display(Name = "Signed Agreement")]
        public string? SignedAgreementPath { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; }
            = new List<ServiceRequest>();

        // ── Observer pattern ──────────────────────────────────────────
        // NotMapped means EF Core ignores this list — not a DB column
        [NotMapped]
        private readonly List<IContractObserver> _observers = new();

        public void RegisterObserver(IContractObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IContractObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task NotifyObservers(Contract contract,
            string changeDescription)
        {
            foreach (var observer in _observers)
            {
                await observer.Update(contract, changeDescription);
            }
        }

        // Call this instead of setting Status directly
        public async Task ChangeStatus(ContractStatus newStatus,
            List<IContractObserver> observers)
        {
            Status = newStatus;
            foreach (var o in observers)
                RegisterObserver(o);

            await NotifyObservers(this,
                $"Status changed to {newStatus}");
        }
    }
}