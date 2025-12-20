using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web_Project.Models
{
    public class PendingAppointmentRow
    {
        public int AppointmentID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsApproved { get; set; }

        public string MemberName { get; set; } = "Unknown";
        public string ServiceName { get; set; } = "Unknown";
        public decimal Fee { get; set; }
    }
}
