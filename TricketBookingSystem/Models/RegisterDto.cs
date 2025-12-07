using System.ComponentModel.DataAnnotations;

namespace TricketBookingSystem.Models
{
    public class RegisterDto
    {
        [Required]
        public string? FullName { get; set; }

        [Required]
        [Phone]
        public string? MobileNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
