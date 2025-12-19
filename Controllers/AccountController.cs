using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Karigar.Data;

namespace Karigar.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> BecomeServiceProvider()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var isServiceProvider = await _userManager.IsInRoleAsync(user, "ServiceProvider");
            if (isServiceProvider)
            {
                return RedirectToAction("MyProfile", "ServiceProviders");
            }

            // Add ServiceProvider role
            await _userManager.AddToRoleAsync(user, "ServiceProvider");
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Create", "ServiceProviders");
        }
    }
}

