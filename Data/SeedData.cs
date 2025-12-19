using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Karigar.Models;

namespace Karigar.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.MigrateAsync();

            // Create roles
            string[] roles = { "Admin", "ServiceProvider", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user
            var adminEmail = "admin@karigar.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Service Providers
            await SeedServiceProviders(context, userManager);
        }

        private static async Task SeedServiceProviders(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            if (await context.ServiceProviders.AnyAsync())
            {
                return; // Already seeded
            }

            var categories = await context.Categories.ToListAsync();
            var providers = new[]
            {
                new { Email = "plumber@karigar.com", Password = "Provider@123", BusinessName = "Expert Plumbing Services", Description = "Professional plumbing solutions for all your needs. 24/7 emergency service available.", City = "Karachi", State = "Sindh", Category = "Plumbing", Services = new[] { ("Pipe Repair", 1500m, "per service"), ("Drain Cleaning", 2000m, "per service"), ("Faucet Installation", 1200m, "per installation") } },
                new { Email = "electrician@karigar.com", Password = "Provider@123", BusinessName = "Safe Electrical Works", Description = "Licensed electrician with 10+ years experience. Safe and reliable electrical services.", City = "Lahore", State = "Punjab", Category = "Electrical", Services = new[] { ("Wiring Installation", 5000m, "per room"), ("Switch Repair", 800m, "per switch"), ("Circuit Breaker Fix", 2500m, "per service") } },
                new { Email = "cleaner@karigar.com", Password = "Provider@123", BusinessName = "Sparkle Cleaning Services", Description = "Professional cleaning services for homes and offices. Eco-friendly products used.", City = "Islamabad", State = "ICT", Category = "Cleaning", Services = new[] { ("Deep Cleaning", 5000m, "per session"), ("Regular Cleaning", 3000m, "per visit"), ("Carpet Cleaning", 4000m, "per room") } },
                new { Email = "carpenter@karigar.com", Password = "Provider@123", BusinessName = "Master Carpentry Works", Description = "Custom furniture and carpentry solutions. Quality craftsmanship guaranteed.", City = "Karachi", State = "Sindh", Category = "Carpentry", Services = new[] { ("Custom Furniture", 15000m, "per piece"), ("Door Installation", 3000m, "per door"), ("Cabinet Repair", 2500m, "per cabinet") } },
                new { Email = "painter@karigar.com", Password = "Provider@123", BusinessName = "Colorful Paint Solutions", Description = "Interior and exterior painting services. Premium quality paints and finishes.", City = "Lahore", State = "Punjab", Category = "Painting", Services = new[] { ("Interior Painting", 8000m, "per room"), ("Exterior Painting", 12000m, "per house"), ("Wall Texture", 5000m, "per room") } },
                new { Email = "acrepair@karigar.com", Password = "Provider@123", BusinessName = "Cool Air AC Services", Description = "AC installation, repair, and maintenance. All brands serviced.", City = "Karachi", State = "Sindh", Category = "AC Repair", Services = new[] { ("AC Installation", 10000m, "per unit"), ("AC Repair", 3000m, "per service"), ("AC Maintenance", 2000m, "per visit") } }
            };

            foreach (var providerData in providers)
            {
                var user = await userManager.FindByEmailAsync(providerData.Email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = providerData.Email,
                        Email = providerData.Email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, providerData.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "ServiceProvider");
                    }
                }

                if (user != null && !await context.ServiceProviders.AnyAsync(sp => sp.UserId == user.Id))
                {
                    var category = categories.FirstOrDefault(c => c.Name == providerData.Category);
                    var serviceProvider = new Karigar.Models.ServiceProvider
                    {
                        UserId = user.Id,
                        BusinessName = providerData.BusinessName,
                        Description = providerData.Description,
                        Address = $"Main Street, {providerData.City}",
                        City = providerData.City,
                        State = providerData.State,
                        ZipCode = "12345",
                        PhoneNumber = "+92-300-1234567",
                        IsApproved = true,
                        IsSuspended = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.ServiceProviders.Add(serviceProvider);
                    await context.SaveChangesAsync();

                    // Add services
                    if (category != null)
                    {
                        foreach (var serviceData in providerData.Services)
                        {
                            var service = new Service
                            {
                                ServiceProviderId = serviceProvider.Id,
                                CategoryId = category.Id,
                                Name = serviceData.Item1,
                                Price = serviceData.Item2,
                                PriceUnit = serviceData.Item3,
                                IsAvailable = true,
                                CreatedAt = DateTime.UtcNow
                            };
                            context.Services.Add(service);
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}

