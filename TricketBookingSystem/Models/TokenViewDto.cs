namespace TricketBookingSystem.Models
{
    public class TokenViewDto
    {
        public int TokenNumber { get; set; }
        public bool IsBooked { get; set; }  // true if any user booked
        public bool IsMine { get; set; }    // true if booked by logged-in user
    }
}
