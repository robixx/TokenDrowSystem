using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TricketBookingSystem.Models
{
    [Table("app_Users")]
    public class Users
    {
        [Key]
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? MobileNumber { get; set; }
        public string? Password { get; set; }
        public string? SaltPassword { get; set; }
        public int Status { get; set; }
    }
}
