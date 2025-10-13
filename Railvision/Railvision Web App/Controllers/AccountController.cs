using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailVision.App.Models;
using System.Security.Claims;
using TrainGenie.Models;

public class AccountController : Controller
{
    private readonly IAntiforgery _antiforgery;
    private readonly ApplicationDbContext _dbContext;

    public AccountController(IAntiforgery antiforgery, ApplicationDbContext dbContext)
    {
        _antiforgery = antiforgery;
        _dbContext = dbContext;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        // Set anti-forgery token in cookie
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append("X-CSRF-TOKEN", tokens.RequestToken!,
            new CookieOptions { HttpOnly = false }); // Must be accessible to JS
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAsync(string role, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "All fields are required.";
            return View("Login");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Role == role);
        if (user != null)
        {
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetString("IsAuthenticated", "true");
                HttpContext.Session.SetString("UserRole", role);
                HttpContext.Session.SetString("Username", username);

                Response.Cookies.Append(
                    "TrainGenie.Auth",
                    "authenticated",
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.Now.AddMinutes(20)
                    }
                );

                // Refresh anti-forgery token after login
                var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
                Response.Cookies.Append("X-CSRF-TOKEN", tokens.RequestToken!,
                    new CookieOptions { HttpOnly = false });

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                };
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
        }

        ViewBag.Error = "Invalid credentials";
        return View("Login");
    }

    [HttpPost]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync("Cookies");
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Server-side email validation for UZ student email
        var regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z]+\.[a-zA-Z]+@students\.uz\.ac\.zw$");
        if (!regex.IsMatch(model.Email ?? ""))
        {
            ViewBag.Error = "Email must be a valid University of Zimbabwe student email (name.surname@students.uz.ac.zw)";
            return View(model);
        }

        // Check if user exists
        var exists = await _dbContext.Users.AnyAsync(u => u.Email == model.Email || u.Username == model.Username);
        if (exists)
        {
            ViewBag.Error = "User with this email or username already exists.";
            return View(model);
        }

        // Hash password
        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Email = model.Email,
            Username = model.Username,
            Role = model.Role
        };
        user.PasswordHash = hasher.HashPassword(user, model.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Optionally, auto-login after registration:
        // return await LoginAsync(user.Role, user.Username, model.Password);

        // Or redirect to login page:
        TempData["Success"] = "Registration successful. Please log in.";
        return RedirectToAction("Login");
    }
}