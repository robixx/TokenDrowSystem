using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TricketBookingSystem.Data;
using TricketBookingSystem.Models;

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

        [HttpPost]
        public async Task<IActionResult> SaveToken([FromBody] TokenBookingDto bookingData)
        {
            var currentDate = DateTime.Now;

            if (bookingData == null)
                return BadRequest(new { message = "No data received!" });

            
            var activeTricket = await _connection.Tricket
                .Where(t => t.IsActive == 1 && t.StartDate <= currentDate && t.EndDate >= currentDate)
                .FirstOrDefaultAsync();

            if (activeTricket == null)
                return BadRequest(new { message = "No active ticket available." });

            
            List<UserTokenSelection> selectedTokens = new List<UserTokenSelection>();

            if (bookingData.Tokens != null && bookingData.Tokens.Count > 0)
            {
                foreach (var token in bookingData.Tokens)
                {
                    selectedTokens.Add(new UserTokenSelection
                    {
                        UserId = 0, 
                        TokenNumber = token,
                        SelectedDate = currentDate,
                        UserName = bookingData.Name,
                        MobileNumber = bookingData.Mobile,
                        TricketId = activeTricket.Id
                    });
                }
            }

            // Save all selected tokens in the database
            await _connection.UserTokenSelection.AddRangeAsync(selectedTokens);
            await _connection.SaveChangesAsync();

            return Json(new { success = true, message = "Tokens booked successfully!" });
        }
    }
}
