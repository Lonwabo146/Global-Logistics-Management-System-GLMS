using Microsoft.EntityFrameworkCore;
using GLMS.Models;
namespace GLMS.Data
{
   
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            // Each DbSet becomes a table in your database
            public DbSet<Customer> Customer { get; set; }
            public DbSet<Contract> Contracts { get; set; }
            public DbSet<ServiceRequest> ServiceRequests { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Customer -> Contracts (one to many)
                modelBuilder.Entity<Contract>()
                    .HasOne(c => c.Customer)
                    .WithMany(cu => cu.Contracts)
                    .HasForeignKey(c => c.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Contract -> ServiceRequests (one to many)
                modelBuilder.Entity<ServiceRequest>()
                    .HasOne(sr => sr.Contract)
                    .WithMany(c => c.ServiceRequests)
                    .HasForeignKey(sr => sr.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Store enums as strings in DB (readable, not integers)
                modelBuilder.Entity<Contract>()
                    .Property(c => c.Status)
                    .HasConversion<string>();

                modelBuilder.Entity<ServiceRequest>()
                    .Property(sr => sr.Status)
                    .HasConversion<string>();
            }
        }
    }

    