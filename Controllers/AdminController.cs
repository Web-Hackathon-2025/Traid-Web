using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karigar.Data;
using Karigar.Models;

namespace Karigar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var stats = new
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalCustomers = (await _userManager.GetUsersInRoleAsync("Customer")).Count,
                TotalServiceProviders = await _context.ServiceProviders.CountAsync(),
                PendingApprovals = await _context.ServiceProviders.CountAsync(sp => !sp.IsApproved),
                SuspendedProviders = await _context.ServiceProviders.CountAsync(sp => sp.IsSuspended),
                TotalServices = await _context.Services.CountAsync(),
                TotalBookings = await _context.ServiceRequests.CountAsync(),
                PendingBookings = await _context.ServiceRequests.CountAsync(sr => sr.Status == RequestStatus.Requested),
                TotalReviews = await _context.Reviews.CountAsync(r => r.IsVisible)
            };

            ViewBag.Stats = stats;
            return View();
        }

        // GET: Admin/ServiceProviders
        public async Task<IActionResult> ServiceProviders(string? search, bool? pendingOnly)
        {
            var query = _context.ServiceProviders
                .Include(sp => sp.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(sp => sp.BusinessName.Contains(search) || 
                                         sp.User!.Email!.Contains(search));
            }

            if (pendingOnly == true)
            {
                query = query.Where(sp => !sp.IsApproved);
            }

            var providers = await query.OrderByDescending(sp => sp.CreatedAt).ToListAsync();
            return View(providers);
        }

        // POST: Admin/ApproveServiceProvider/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveServiceProvider(int id)
        {
            var serviceProvider = await _context.ServiceProviders.FindAsync(id);
            if (serviceProvider == null)
            {
                return NotFound();
            }

            serviceProvider.IsApproved = true;
            serviceProvider.IsSuspended = false;
            serviceProvider.UpdatedAt = DateTime.UtcNow;

            _context.Update(serviceProvider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ServiceProviders));
        }

        // POST: Admin/SuspendServiceProvider/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendServiceProvider(int id)
        {
            var serviceProvider = await _context.ServiceProviders.FindAsync(id);
            if (serviceProvider == null)
            {
                return NotFound();
            }

            serviceProvider.IsSuspended = true;
            serviceProvider.UpdatedAt = DateTime.UtcNow;

            _context.Update(serviceProvider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ServiceProviders));
        }

        // POST: Admin/RemoveServiceProvider/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveServiceProvider(int id)
        {
            var serviceProvider = await _context.ServiceProviders.FindAsync(id);
            if (serviceProvider == null)
            {
                return NotFound();
            }

            _context.ServiceProviders.Remove(serviceProvider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ServiceProviders));
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users(string? search, string? role)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.Email!.Contains(search) || u.UserName!.Contains(search));
            }

            var userList = await users.ToListAsync();
            var userViewModels = new List<object>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "Customer";

                if (!string.IsNullOrEmpty(role) && userRole != role)
                {
                    continue;
                }

                var serviceProvider = await _context.ServiceProviders
                    .FirstOrDefaultAsync(sp => sp.UserId == user.Id);

                userViewModels.Add(new
                {
                    User = user,
                    Role = userRole,
                    ServiceProvider = serviceProvider
                });
            }

            ViewBag.Role = role;
            return View(userViewModels);
        }

        // GET: Admin/Reviews
        public async Task<IActionResult> Reviews(string? search)
        {
            var query = _context.Reviews
                .Include(r => r.ServiceProvider)
                .Include(r => r.ServiceRequest)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.ServiceProvider.BusinessName.Contains(search) ||
                                        r.Comment!.Contains(search));
            }

            var reviews = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(reviews);
        }

        // POST: Admin/ToggleReviewVisibility/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleReviewVisibility(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            review.IsVisible = !review.IsVisible;
            review.UpdatedAt = DateTime.UtcNow;

            _context.Update(review);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Reviews));
        }

        // GET: Admin/Bookings
        public async Task<IActionResult> Bookings(string? status)
        {
            var query = _context.ServiceRequests
                .Include(sr => sr.Service)
                .Include(sr => sr.ServiceProvider)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RequestStatus>(status, out var statusEnum))
            {
                query = query.Where(sr => sr.Status == statusEnum);
            }

            var bookings = await query.OrderByDescending(sr => sr.CreatedAt).ToListAsync();
            ViewBag.Status = status;
            return View(bookings);
        }
    }
}

