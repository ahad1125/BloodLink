using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Project.Models;

namespace Web_Project.Controllers;

public class HomeController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext db) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Privacy() => View();
    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Dashboard));
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);
            return RedirectToAction(nameof(Dashboard));
        }

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "This account is temporarily locked after too many failed attempts. Try again later."
            : "Invalid email or password.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Dashboard));
        return View(new RegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role == "Donor" && string.IsNullOrWhiteSpace(model.BloodGroup))
            ModelState.AddModelError(nameof(model.BloodGroup), "Select your blood group.");
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            PhoneNumber = model.Phone?.Trim(),
            FullName = model.FullName.Trim(),
            City = model.City.Trim(),
            AccountType = model.Role,
            BloodGroup = model.Role == "Donor" ? model.BloodGroup : null
        };

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        if (!await roleManager.RoleExistsAsync(model.Role))
            await roleManager.CreateAsync(new IdentityRole(model.Role));
        var roleResult = await userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            foreach (var error in roleResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        string view;
        if (await userManager.IsInRoleAsync(user, "Administrator")) view = "Admin";
        else if (await userManager.IsInRoleAsync(user, "Hospital")) view = "Hospital";
        else if (await userManager.IsInRoleAsync(user, "Donor")) view = "Donor";
        else if (await userManager.IsInRoleAsync(user, "Requester")) view = "Requester";
        else return Forbid();

        ViewData["DisplayName"] = user.FullName;
        ViewData["City"] = user.City;
        ViewData["BloodGroup"] = user.BloodGroup ?? "Not set";
        if (view == "Requester")
            ViewBag.Requests = await db.BloodRequests.Where(request => request.RequesterId == user.Id)
                .OrderByDescending(request => request.CreatedAtUtc).Take(10).ToListAsync();
        return View(view);
    }

    [Authorize(Roles = "Requester")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestBlood(string bloodGroup, int units, string hospital)
    {
        var validGroups = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
        if (!validGroups.Contains(bloodGroup) || units is < 1 or > 20 || string.IsNullOrWhiteSpace(hospital))
        {
            TempData["Notice"] = "Enter a valid blood group, unit count (1–20), and hospital.";
            return RedirectToAction(nameof(Dashboard));
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        db.BloodRequests.Add(new BloodRequest
        {
            BloodGroup = bloodGroup,
            Units = units,
            Hospital = hospital.Trim(),
            RequesterId = user.Id
        });
        await db.SaveChangesAsync();
        TempData["Notice"] = "Your blood request has been saved.";
        return RedirectToAction(nameof(Dashboard));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}
