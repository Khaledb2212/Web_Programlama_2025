using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web_API.Models
{
    public class TrainerAvailability
    {
        [Key]
        public int AvailabilityId { get; set; }        
        


        [Range(0, 6, ErrorMessage = "DayOfWeek must be between 0 (Sunday) and 6 (Saturday).")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.Time)] // <--- Crucial: Makes the browser show a Clock/Time picker
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }



        [Required]
        [Display(Name = "Trainer")]
        public int TrainerId { get; set; }
        [ForeignKey("TrainerId")]
        public virtual Trainer? Trainer { get; set; } 


        [Required]
        [Display(Name = "Service Type")]
        public int ServiceTypeId { get; set; }

        [ForeignKey("ServiceTypeId")]
        public virtual Service? Service { get; set; } 


        public TrainerAvailability() { }
    }
}
