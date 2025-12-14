using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

public class BookAppointmentVm
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required]
    public int ServiceId { get; set; }

    public int? TrainerId { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public List<SelectListItem> Services { get; set; } = new();
    public List<SelectListItem> Trainers { get; set; } = new();
}
