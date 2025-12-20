using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web_Project.Models;

namespace Web_Project.Models
{
    public class TrainerPortalVm
    {
        public string? ExpertiseAreas { get; set; }
        public string? Description { get; set; }

        // ---- Add slot form ----
        [Range(0, 6)]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; } 

        [Required]
        public TimeSpan EndTime { get; set; }   

        [Required]
        public int ServiceTypeId { get; set; }  

        public List<SelectListItem> Services { get; set; } = new();

        // ---- Data tables ----
        public List<MySlotRow> MySlots { get; set; } = new();
        public List<PendingAppointmentRow> PendingAppointments { get; set; } = new();
        public List<PendingAppointmentRow> UpcomingApprovedAppointments { get; set; } = new();

    }

}
