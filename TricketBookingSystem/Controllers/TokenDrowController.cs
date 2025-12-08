using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TricketBookingSystem.Data;
using TricketBookingSystem.Models;

namespace TricketBookingSystem.Controllers
{
    public class TokenDrowController : Controller
    {
        private readonly DatabaseConnection _connection;
        public TokenDrowController( DatabaseConnection connection)
        {
           
            _connection = connection;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentDate = DateTime.Now;

            // Get active ticket
            var activeTricket = await _connection.Tricket
                .Where(t => t.IsActive == 1 && t.StartDate <= currentDate && t.EndDate >= currentDate)
                .FirstOrDefaultAsync();

            if (activeTricket == null)
                return View(new List<string>());         

            // Get distinct booked token numbers for this ticket
            var bookedTokens = await _connection.UserTokenSelection
                .Where(x => x.TricketId == activeTricket.Id)
                .Select(x => x.TokenNumber)
                .Distinct()
                .ToListAsync();

           

            ViewBag.AvailableTokens = bookedTokens;
            ViewBag.BookedTokens = bookedTokens;
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = 0;
            if (User.Identity.IsAuthenticated)
                userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            DateTime today = DateTime.Now.Date;

            // 1. Get current active event
            var eventData = await _connection.Tricket
                .Where(t => t.StartDate <= today && t.EndDate >= today && t.IsActive == 1)
                .FirstOrDefaultAsync();

            if (eventData == null)
            {
                ViewBag.EventDate = "";
                ViewBag.TotalOwned = 0;
                ViewBag.Purchased = new List<int>();
                ViewBag.TotalTickets = 0;
                return View();
            }

            ViewBag.EventDate = eventData.StartDate?.ToString("dd-MMM-yyyy");
            ViewBag.TotalTickets = eventData.TotalTricket ?? 0;          
         

            // 2. Load user's purchased tickets
            var purchased = await _connection.UserTokenSelection
                
                .Select(x => x.TokenNumber)
                .ToListAsync();

            ViewBag.Purchased = purchased;
            ViewBag.TotalOwned = purchased.Count;
            return View();
        }
    }
}
