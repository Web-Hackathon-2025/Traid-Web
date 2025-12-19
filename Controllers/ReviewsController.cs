using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karigar.Data;
using Karigar.Models;

namespace Karigar.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reviews/Create
        public async Task<IActionResult> Create(int? serviceRequestId)
        {
            if (serviceRequestId == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var serviceRequest = await _context.ServiceRequests
                .Include(sr => sr.Service)
                .Include(sr => sr.ServiceProvider)
                .FirstOrDefaultAsync(sr => sr.Id == serviceRequestId && 
                    sr.CustomerId == userId && 
                    sr.Status == RequestStatus.Completed &&
                    sr.Review == null);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            ViewBag.ServiceRequest = serviceRequest;
            return View();
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceRequestId,Rating,Comment")] Review review)
        {
            var userId = _userManager.GetUserId(User);
            var serviceRequest = await _context.ServiceRequests
                .Include(sr => sr.ServiceProvider)
                .FirstOrDefaultAsync(sr => sr.Id == review.ServiceRequestId && 
                    sr.CustomerId == userId && 
                    sr.Status == RequestStatus.Completed &&
                    sr.Review == null);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                review.CustomerId = userId!;
                review.ServiceProviderId = serviceRequest.ServiceProviderId;
                review.CreatedAt = DateTime.UtcNow;
                review.IsVisible = true;

                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Bookings", new { id = review.ServiceRequestId });
            }

            ViewBag.ServiceRequest = serviceRequest;
            return View(review);
        }
    }
}

