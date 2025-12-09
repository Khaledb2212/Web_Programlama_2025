using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web_API.Models
{
    public class Trainer
    {
        [Key] 
        public int TrainerID { get; set; }


        public int PersonID { get; set; }
        [ForeignKey("PersonID")]
        public Person? person { get; set; }

        [Required(ErrorMessage = "Expertise areas are required.")]
        [StringLength(500, ErrorMessage = "Expertise areas cannot exceed 500 characters.")]
        public string? ExpertiseAreas { get; set; }


        public virtual ICollection<TrainerSkill>? Skills { get; set; }



        

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; } // Fixed typo from image "Descrption"



        // 1. Empty Constructor (Required for Entity Framework)
        public Trainer()
        {
            this.Skills = null;
        }

        // 2. Full Constructor (For creating objects manually)
        public Trainer(int trainerId, int personId, string expertiseAreas, string description)
        {
            this.TrainerID = trainerId;
            this.PersonID = personId;
            this.ExpertiseAreas = expertiseAreas;
            this.Description = description;
        }
    }
}

