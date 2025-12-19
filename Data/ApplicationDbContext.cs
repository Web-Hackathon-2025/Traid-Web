using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Karigar.Models;
using ServiceProviderModel = Karigar.Models.ServiceProvider;

namespace Karigar.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ServiceProviderModel> ServiceProviders { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<ServiceRequest> ServiceRequests { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Availability> Availabilities { get; set; } = null!;
        public DbSet<Dispute> Disputes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ServiceProvider
            builder.Entity<ServiceProviderModel>()
                .HasIndex(sp => sp.UserId)
                .IsUnique();

            // Configure Service
            builder.Entity<Service>()
                .HasOne(s => s.ServiceProvider)
                .WithMany()
                .HasForeignKey(s => s.ServiceProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Service>()
                .HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ServiceRequest
            builder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Service)
                .WithMany()
                .HasForeignKey(sr => sr.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceRequest>()
                .HasOne(sr => sr.ServiceProvider)
                .WithMany()
                .HasForeignKey(sr => sr.ServiceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Review)
                .WithOne(r => r.ServiceRequest)
                .HasForeignKey<Review>(r => r.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Review
            builder.Entity<Review>()
                .HasOne(r => r.ServiceProvider)
                .WithMany()
                .HasForeignKey(r => r.ServiceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Availability
            builder.Entity<Availability>()
                .HasOne(a => a.ServiceProvider)
                .WithMany()
                .HasForeignKey(a => a.ServiceProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Dispute
            builder.Entity<Dispute>()
                .HasOne(d => d.ServiceRequest)
                .WithMany()
                .HasForeignKey(d => d.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Plumbing", Description = "Plumbing services", Icon = "🔧" },
                new Category { Id = 2, Name = "Electrical", Description = "Electrical services", Icon = "⚡" },
                new Category { Id = 3, Name = "Cleaning", Description = "Cleaning services", Icon = "🧹" },
                new Category { Id = 4, Name = "Carpentry", Description = "Carpentry services", Icon = "🪚" },
                new Category { Id = 5, Name = "Painting", Description = "Painting services", Icon = "🎨" },
                new Category { Id = 6, Name = "AC Repair", Description = "Air conditioning repair", Icon = "❄️" },
                new Category { Id = 7, Name = "Appliance Repair", Description = "Appliance repair services", Icon = "🔌" },
                new Category { Id = 8, Name = "Tiles & Flooring", Description = "Tiles and flooring services", Icon = "🧱" }
            );
        }
    }
}
