using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Web_Project.Models
{
    public class MySlotRow
    {
        public int AvailabilityId { get; set; }
        public int DayOfWeek { get; set; }
        public string StartTime { get; set; } = ""; // "HH:mm"
        public string EndTime { get; set; } = "";   // "HH:mm"
        public int ServiceTypeId { get; set; }
        public string? ServiceName { get; set; }
    }
}
