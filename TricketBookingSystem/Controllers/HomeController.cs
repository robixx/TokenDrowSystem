using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TricketBookingSystem.Data;
using TricketBookingSystem.Models;

namespace TricketBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseConnection _connection;
        public HomeController(ILogger<HomeController> logger, DatabaseConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        public async Task<IActionResult> Index()
        {
            var currentDate = DateTime.Now;
            int userId = 0;

            if (User.Identity.IsAuthenticated)
                userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            // Get active Tricket (current period)
            var activeTricket = await _connection.Tricket
                .Where(t => t.IsActive == 1 &&
                            t.StartDate <= currentDate &&
                            t.EndDate >= currentDate)
                .FirstOrDefaultAsync();

            int totalTickets = activeTricket?.TotalTricket ?? 0;

            // Get all booked tokens for this tricket
            var bookedTokens = new List<dynamic>();
            if (activeTricket != null)
            {
                bookedTokens = await _connection.UserTokenSelection
                    .Where(x => x.TricketId == activeTricket.Id) // make sure foreign key is correct
                    .Select(x => new { x.TokenNumber, x.UserId })
                    .ToListAsync<dynamic>();
            }

            ViewBag.TotalTickets = totalTickets;
            ViewBag.BookedTokens = bookedTokens;
            ViewBag.CurrentUserId = userId;
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                // Generate hash + salt
                var (hash, salt) = HashPassword(model.Password ?? "");

                var user = new Users
                {
                    UserName = model.FullName,
                    MobileNumber = model.MobileNumber,
                    Password = hash,
                    SaltPassword = salt,
                    Status=1
                };

                await _connection.Users.AddAsync(user);
                await _connection.SaveChangesAsync();
                TempData["SuccessMessage"] = "Registration successful! You can now login.";
                return RedirectToAction("TokenBooking","TokenBook");
            }

            return View(model);
        }

        public (string Hash, string Salt) HashPassword(string password)
        {
            // Generate random salt
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            var salt = Convert.ToBase64String(saltBytes);

            // Combine password + salt
            var pb = Encoding.UTF8.GetBytes(password + salt);
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(pb);
                var hash = Convert.ToBase64String(hashBytes);
                return (hash, salt);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (ModelState.IsValid)
            {
                // Find user by mobile or username
                var user = await _connection.Users
                    .FirstOrDefaultAsync(u => u.MobileNumber == model.MobileNumber);

                if (user != null)
                {
                    // Hash input password with stored salt
                    var hashedInput = HashWithSalt(model.Password, user.SaltPassword);

                    if (hashedInput == user.Password)
                    {
                        // Sign in user with cookie authentication
                        var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, user.UserName),
                                new Claim("UserId", user.UserId.ToString())
                            };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        return Json(new { success = true, username = user.UserName });
                    }
                }

                return Json(new { success = false, message = "Invalid credentials" });
            }

            return Json(new { success = false, message = "Invalid data" });
        }

        private string HashWithSalt(string password, string salt)
        {
            var pb = Encoding.UTF8.GetBytes(password + salt);
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(pb);
                return Convert.ToBase64String(hashBytes);
            }
        }



        [HttpPost]
      
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("TokenBooking","TokenBook");
        }


        [HttpPost]
        public async Task<IActionResult> SaveSelectedTokens([FromBody] SelectedTokensDto model)
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false, message = "User not logged in!" });

            int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            if (model.Tokens == null || !model.Tokens.Any())
                return Json(new { success = false, message = "No tokens selected!" });

            var currentDate = DateTime.Now;

            // Get active ticket
            var activeTricket = await _connection.Tricket
                .Where(t => t.IsActive == 1 && t.StartDate <= currentDate && t.EndDate >= currentDate)
                .FirstOrDefaultAsync();

            if (activeTricket == null)
                return Json(new { success = false, message = "No active ticket available" });

            // Delete previous selections for this user and ticket
            var previousSelections = await _connection.UserTokenSelection
                .Where(x => x.UserId == userId && x.TricketId == activeTricket.Id)
                .ToListAsync();

            if (previousSelections.Any())
            {
                _connection.UserTokenSelection.RemoveRange(previousSelections);
                await _connection.SaveChangesAsync();
            }

            // Insert new selections
            var newSelections = model.Tokens.Distinct() // remove duplicates if any
                .Select(token => new UserTokenSelection
                {
                    UserId = userId,
                    TokenNumber = token,
                    TricketId = activeTricket.Id,
                    SelectedDate = DateTime.Now
                }).ToList();

            await _connection.UserTokenSelection.AddRangeAsync(newSelections);
            await _connection.SaveChangesAsync();

            return Json(new { success = true, bookedTokens = newSelections.Select(x => x.TokenNumber).ToList() });
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
