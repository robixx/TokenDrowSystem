using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TricketBookingSystem.Models
{
    [Table("app_Trickets")]
    public class Tricket
    {
        [Key]
        public int Id { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? TotalTricket { get; set; }

        public int? IsActive { get; set; }
    }
}
