using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web_API.Models
{
    public class TrainerSkill
    {
        [Key]
        public int Id { get; set; }

        public int TrainerId { get; set; }
        [ForeignKey("TrainerId")]
        public virtual Trainer? Trainer { get; set; } 

        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service? service { get; set; }

    }
}
