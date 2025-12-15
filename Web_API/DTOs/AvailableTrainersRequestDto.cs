using System.ComponentModel.DataAnnotations;

namespace Web_API.DTOs
{
    public class AvailableTrainersRequestDto
    {
        [Required]
        public DateTime Date { get; set; }          // e.g. "2025-12-15"

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public string? Start { get; set; }      // "09:00"

        [Required]
        public string? End { get; set; }     // "10:00"
    }
}
