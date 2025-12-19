using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Karigar.Data;
using Karigar.Models;

namespace Karigar.Controllers
{
    [Authorize(Roles = "ServiceProvider")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ServicesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Services/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (serviceProvider == null)
            {
                return RedirectToAction("Create", "ServiceProviders");
            }

            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,PriceUnit,CategoryId,IsAvailable,EstimatedDurationMinutes")] Service service)
        {
            var userId = _userManager.GetUserId(User);
            var serviceProvider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (serviceProvider == null)
            {
                return RedirectToAction("Create", "ServiceProviders");
            }

            if (ModelState.IsValid)
            {
                service.ServiceProviderId = serviceProvider.Id;
                service.CreatedAt = DateTime.UtcNow;
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction("MyProfile", "ServiceProviders");
            }

            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Categories.ToListAsync(), "Id", "Name", service.CategoryId);
            return View(service);
        }

        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var service = await _context.Services
                .Include(s => s.ServiceProvider)
                .FirstOrDefaultAsync(s => s.Id == id && s.ServiceProvider.UserId == userId);

            if (service == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Categories.ToListAsync(), "Id", "Name", service.CategoryId);
            return View(service);
        }

        // POST: Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,PriceUnit,CategoryId,IsAvailable,EstimatedDurationMinutes")] Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var existingService = await _context.Services
                .Include(s => s.ServiceProvider)
                .FirstOrDefaultAsync(s => s.Id == id && s.ServiceProvider.UserId == userId);

            if (existingService == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingService.Name = service.Name;
                    existingService.Description = service.Description;
                    existingService.Price = service.Price;
                    existingService.PriceUnit = service.PriceUnit;
                    existingService.CategoryId = service.CategoryId;
                    existingService.IsAvailable = service.IsAvailable;
                    existingService.EstimatedDurationMinutes = service.EstimatedDurationMinutes;
                    existingService.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingService);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction("MyProfile", "ServiceProviders");
            }

            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Categories.ToListAsync(), "Id", "Name", service.CategoryId);
            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var service = await _context.Services
                .Include(s => s.ServiceProvider)
                .FirstOrDefaultAsync(s => s.Id == id && s.ServiceProvider.UserId == userId);

            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyProfile", "ServiceProviders");
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}

