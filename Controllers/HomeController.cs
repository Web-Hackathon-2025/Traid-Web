using System.Diagnostics;
using Karigar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karigar.Data;

namespace Karigar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Services)
                    .ThenInclude(s => s.ServiceProvider)
                .ToListAsync();

            var featuredProviders = await _context.ServiceProviders
                .Where(sp => sp.IsApproved && !sp.IsSuspended)
                .Include(sp => sp.Services)
                .Take(6)
                .ToListAsync();

            var featuredProvidersWithRatings = featuredProviders.Select(sp => new
            {
                Provider = sp,
                AverageRating = _context.Reviews
                    .Where(r => r.ServiceProviderId == sp.Id && r.IsVisible)
                    .Select(r => (double?)r.Rating)
                    .DefaultIfEmpty()
                    .Average() ?? 0,
                TotalReviews = _context.Reviews.Count(r => r.ServiceProviderId == sp.Id && r.IsVisible)
            }).ToList();

            ViewBag.Categories = categories;
            ViewBag.FeaturedProviders = featuredProvidersWithRatings;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
