using System.ComponentModel.DataAnnotations;


namespace GLMS.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Contact Details")]
        public string ContactDetails { get; set; }

        [Required]
        public string Region { get; set; }

        // Navigation property — one Client has many Contracts
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}

