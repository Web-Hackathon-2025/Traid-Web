using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karigar.Data;
using Karigar.Models;
using ServiceProviderModel = Karigar.Models.ServiceProvider;

namespace Karigar.Controllers
{
    public class ServiceProvidersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ServiceProvidersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ServiceProviders
        public async Task<IActionResult> Index(string? search, int? categoryId, string? city)
        {
            var query = _context.ServiceProviders
                .Include(sp => sp.Services)
                    .ThenInclude(s => s.Category)
                .Where(sp => sp.IsApproved && !sp.IsSuspended);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(sp => sp.BusinessName.Contains(search) ||
                                         sp.Services.Any(s => s.Name.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(sp => sp.Services.Any(s => s.CategoryId == categoryId));
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(sp => sp.City.Contains(city));
            }

            var providers = await query.ToListAsync();

            var viewModels = providers.Select(sp => new ServiceProviderViewModel
            {
                Id = sp.Id,
                BusinessName = sp.BusinessName,
                Description = sp.Description,
                Address = sp.Address,
                City = sp.City,
                State = sp.State,
                ZipCode = sp.ZipCode,
                PhoneNumber = sp.PhoneNumber,
                ProfileImageUrl = sp.ProfileImageUrl,
                Services = sp.Services.Where(s => s.IsAvailable).Select(s => new ServiceViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    PriceUnit = s.PriceUnit,
                    IsAvailable = s.IsAvailable,
                    CategoryName = s.Category?.Name ?? ""
                }).ToList(),
                AverageRating = _context.Reviews
                    .Where(r => r.ServiceProviderId == sp.Id && r.IsVisible)
                    .Select(r => (double?)r.Rating)
                    .DefaultIfEmpty()
                    .Average(),
                TotalReviews = _context.Reviews.Count(r => r.ServiceProviderId == sp.Id && r.IsVisible)
            }).ToList();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.City = city;

            return View(viewModels);
        }

        // GET: ServiceProviders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceProvider = await _context.ServiceProviders
                .Include(sp => sp.Services)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceProvider == null || (!serviceProvider.IsApproved && !User.IsInRole("Admin")))
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .Where(r => r.ServiceProviderId == id && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            var viewModel = new ServiceProviderViewModel
            {
                Id = serviceProvider.Id,
                BusinessName = serviceProvider.BusinessName,
                Description = serviceProvider.Description,
                Address = serviceProvider.Address,
                City = serviceProvider.City,
                State = serviceProvider.State,
                ZipCode = serviceProvider.ZipCode,
                PhoneNumber = serviceProvider.PhoneNumber,
                ProfileImageUrl = serviceProvider.ProfileImageUrl,
                Services = serviceProvider.Services.Where(s => s.IsAvailable).Select(s => new ServiceViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    PriceUnit = s.PriceUnit,
                    IsAvailable = s.IsAvailable,
                    CategoryName = s.Category?.Name ?? ""
                }).ToList(),
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : null,
                TotalReviews = reviews.Count
            };

            ViewBag.Reviews = reviews;
            ViewBag.IsCustomer = User.IsInRole("Customer");

            return View(viewModel);
        }

        // GET: ServiceProviders/Create
        [Authorize(Roles = "ServiceProvider")]
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            var existingProvider = _context.ServiceProviders.FirstOrDefault(sp => sp.UserId == userId);

            if (existingProvider != null)
            {
                return RedirectToAction(nameof(Edit));
            }

            return View();
        }

        // POST: ServiceProviders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Create([Bind("BusinessName,Description,Address,City,State,ZipCode,PhoneNumber")] ServiceProviderModel serviceProvider)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                serviceProvider.UserId = userId!;
                serviceProvider.CreatedAt = DateTime.UtcNow;

                _context.Add(serviceProvider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyProfile));
            }
            return View(serviceProvider);
        }

        // GET: ServiceProviders/Edit
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (serviceProvider == null)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(serviceProvider);
        }

        // POST: ServiceProviders/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BusinessName,Description,Address,City,State,ZipCode,PhoneNumber")] ServiceProviderModel serviceProvider)
        {
            if (id != serviceProvider.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var existingProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.UserId == userId);

            if (existingProvider == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingProvider.BusinessName = serviceProvider.BusinessName;
                    existingProvider.Description = serviceProvider.Description;
                    existingProvider.Address = serviceProvider.Address;
                    existingProvider.City = serviceProvider.City;
                    existingProvider.State = serviceProvider.State;
                    existingProvider.ZipCode = serviceProvider.ZipCode;
                    existingProvider.PhoneNumber = serviceProvider.PhoneNumber;
                    existingProvider.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingProvider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceProviderExists(serviceProvider.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(MyProfile));
            }
            return View(serviceProvider);
        }

        // GET: ServiceProviders/MyProfile
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> MyProfile()
        {
            var userId = _userManager.GetUserId(User);
            var serviceProvider = await _context.ServiceProviders
                .Include(sp => sp.Services)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (serviceProvider == null)
            {
                return RedirectToAction(nameof(Create));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(serviceProvider);
        }

        private bool ServiceProviderExists(int id)
        {
            return _context.ServiceProviders.Any(e => e.Id == id);
        }
    }
}

