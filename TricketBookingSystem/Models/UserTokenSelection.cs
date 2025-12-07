using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TricketBookingSystem.Models
{
    [Table("app_UserTokenSelection")]
    public class UserTokenSelection
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TricketId { get; set; }
        public int TokenNumber { get; set; }
        public DateTime SelectedDate { get; set; }
    }
}
