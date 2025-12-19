using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karigar.Data;
using Karigar.Models;

namespace Karigar.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bookings/Create
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(int serviceId)
        {
            var service = await _context.Services
                .Include(s => s.ServiceProvider)
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null || !service.IsAvailable)
            {
                return NotFound();
            }

            ViewBag.Service = service;
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([Bind("ServiceId,ServiceAddress,SpecialInstructions,RequestedDate")] ServiceRequest serviceRequest)
        {
            var userId = _userManager.GetUserId(User);
            var service = await _context.Services
                .Include(s => s.ServiceProvider)
                .FirstOrDefaultAsync(s => s.Id == serviceRequest.ServiceId);

            if (service == null || !service.IsAvailable)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                serviceRequest.CustomerId = userId!;
                serviceRequest.ServiceProviderId = service.ServiceProviderId;
                serviceRequest.Status = RequestStatus.Requested;
                serviceRequest.CreatedAt = DateTime.UtcNow;

                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyBookings));
            }

            ViewBag.Service = service;
            return View(serviceRequest);
        }

        // GET: Bookings/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var isServiceProvider = await _userManager.IsInRoleAsync(user!, "ServiceProvider");

            IQueryable<ServiceRequest> query;

            if (isServiceProvider)
            {
                var serviceProvider = await _context.ServiceProviders
                    .FirstOrDefaultAsync(sp => sp.UserId == userId);

                if (serviceProvider == null)
                {
                    return RedirectToAction("Create", "ServiceProviders");
                }

                query = _context.ServiceRequests
                    .Include(sr => sr.Service)
                        .ThenInclude(s => s.Category)
                    .Include(sr => sr.ServiceProvider)
                    .Where(sr => sr.ServiceProviderId == serviceProvider.Id)
                    .OrderByDescending(sr => sr.CreatedAt);
            }
            else
            {
                query = _context.ServiceRequests
                    .Include(sr => sr.Service)
                        .ThenInclude(s => s.Category)
                    .Include(sr => sr.ServiceProvider)
                    .Where(sr => sr.CustomerId == userId)
                    .OrderByDescending(sr => sr.CreatedAt);
            }

            var bookings = await query.ToListAsync();
            ViewBag.IsServiceProvider = isServiceProvider;

            return View(bookings);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var isServiceProvider = await _userManager.IsInRoleAsync(user!, "ServiceProvider");

            var serviceRequest = await _context.ServiceRequests
                .Include(sr => sr.Service)
                    .ThenInclude(s => s.Category)
                .Include(sr => sr.ServiceProvider)
                .Include(sr => sr.Review)
                .FirstOrDefaultAsync(sr => sr.Id == id &&
                    (sr.CustomerId == userId || (isServiceProvider && sr.ServiceProvider.UserId == userId)));

            if (serviceRequest == null)
            {
                return NotFound();
            }

            ViewBag.IsServiceProvider = isServiceProvider;
            ViewBag.CanReview = !isServiceProvider && 
                                serviceRequest.Status == RequestStatus.Completed && 
                                serviceRequest.Review == null;

            return View(serviceRequest);
        }

        // POST: Bookings/Accept/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Accept(int id, DateTime? scheduledDate)
        {
            var userId = _userManager.GetUserId(User);
            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (serviceProvider == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.Id == id && sr.ServiceProviderId == serviceProvider.Id);

            if (serviceRequest == null || serviceRequest.Status != RequestStatus.Requested)
            {
                return NotFound();
            }

            serviceRequest.Status = RequestStatus.Confirmed;
            serviceRequest.ConfirmedDate = DateTime.UtcNow;
            serviceRequest.ScheduledDate = scheduledDate;
            serviceRequest.UpdatedAt = DateTime.UtcNow;

            _context.Update(serviceRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Bookings/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Reject(int id, string? cancellationReason)
        {
            var userId = _userManager.GetUserId(User);
            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (serviceProvider == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.Id == id && sr.ServiceProviderId == serviceProvider.Id);

            if (serviceRequest == null || serviceRequest.Status != RequestStatus.Requested)
            {
                return NotFound();
            }

            serviceRequest.Status = RequestStatus.Rejected;
            serviceRequest.CancelledDate = DateTime.UtcNow;
            serviceRequest.CancellationReason = cancellationReason;
            serviceRequest.UpdatedAt = DateTime.UtcNow;

            _context.Update(serviceRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Bookings/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Complete(int id, decimal? finalPrice)
        {
            var userId = _userManager.GetUserId(User);
            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (serviceProvider == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(sr => sr.Service)
                .FirstOrDefaultAsync(sr => sr.Id == id && sr.ServiceProviderId == serviceProvider.Id);

            if (serviceRequest == null || serviceRequest.Status != RequestStatus.Confirmed)
            {
                return NotFound();
            }

            serviceRequest.Status = RequestStatus.Completed;
            serviceRequest.CompletedDate = DateTime.UtcNow;
            serviceRequest.FinalPrice = finalPrice ?? serviceRequest.Service?.Price;
            serviceRequest.UpdatedAt = DateTime.UtcNow;

            _context.Update(serviceRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Bookings/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? cancellationReason)
        {
            var userId = _userManager.GetUserId(User);
            var serviceRequest = await _context.ServiceRequests
                .Include(sr => sr.ServiceProvider)
                .FirstOrDefaultAsync(sr => sr.Id == id && 
                    (sr.CustomerId == userId || sr.ServiceProvider.UserId == userId));

            if (serviceRequest == null || 
                (serviceRequest.Status != RequestStatus.Requested && serviceRequest.Status != RequestStatus.Confirmed))
            {
                return NotFound();
            }

            serviceRequest.Status = RequestStatus.Cancelled;
            serviceRequest.CancelledDate = DateTime.UtcNow;
            serviceRequest.CancellationReason = cancellationReason;
            serviceRequest.UpdatedAt = DateTime.UtcNow;

            _context.Update(serviceRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

