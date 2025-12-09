using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web_API.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }


        [Required]
        public int MemberID { get; set; }

        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; } 


        [Required]
        public int TrainerID { get; set; }

        [ForeignKey("TrainerID")]
        public virtual Trainer? Trainer { get; set; } 



        [Required]
        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public virtual Service? Service { get; set; }


        // --- Time Details ---
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal Fee { get; set; }


        public bool IsApproved { get; set; }

        public Appointment()
        {
        }
    }
}
