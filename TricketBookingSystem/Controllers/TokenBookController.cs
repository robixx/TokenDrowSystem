using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TricketBookingSystem.Data;

namespace TricketBookingSystem.Controllers
{
    public class TokenBookController : Controller
    {
        private readonly DatabaseConnection _connection;
        public TokenBookController(DatabaseConnection connection)
        {
           
            _connection = connection;
        }

        public async Task<IActionResult> TokenBooking()
        {
            var currentDate = DateTime.Now;
           

            // Get active Tricket (current period)
            var activeTricket = await _connection.Tricket
                .Where(t => t.IsActive == 1 &&
                            t.StartDate <= currentDate &&
                            t.EndDate >= currentDate)
                .FirstOrDefaultAsync();

            int totalTickets = activeTricket?.TotalTricket ?? 0;
            int distance = activeTricket?.TokenDistance ?? 0;

            // Get all booked tokens for this tricket
            var bookedTokens = new List<dynamic>();
            if (activeTricket != null)
            {
                bookedTokens = await _connection.UserTokenSelection
                    .Where(x => x.TricketId == activeTricket.Id) // make sure foreign key is correct
                    .Select(x => new { x.TokenNumber })
                    .ToListAsync<dynamic>();
            }

            ViewBag.TotalTickets = totalTickets;
            ViewBag.Distance = distance;
            ViewBag.BookedTokens = bookedTokens;
            
            return View();
        }
    }
}
