using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Models
{
    public enum ServiceRequestStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost (USD)")]
        public decimal CostUSD { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost (ZAR)")]
        public decimal CostZAR { get; set; }

        [Required]
        public ServiceRequestStatus Status { get; set; }

        // Foreign key to Contract
        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }

        // Navigation property
        public Contract? Contract { get; set; }
    }
}

