
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Web_Project.Models
{
    public class TrainerDTO
    {
        // --- Standard Trainer Data ---
        [JsonPropertyName("trainerID")]
        public int TrainerID { get; set; }

        [JsonPropertyName("personID")]
        public int PersonID { get; set; }

        public string? FullName => $"{FirstName} {LastName}";

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        // Made nullable so it is not required during "Edit"
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [Required]
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [Required]
        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [Required]
        [JsonPropertyName("expertiseAreas")]
        public string? ExpertiseAreas { get; set; }

        [Required]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        // --- SKILLS MANAGEMENT ---

        // 1. For the "Create Trainer" Form:
        // Holds the IDs of the checkboxes checked by the user
        public List<int> SelectedServiceIds { get; set; } = new();

        // 2. For the "Edit Trainer" Page:
        // Holds the list of skills the trainer ALREADY has (displayed in the table)
        public List<TrainerSkillItem> AssignedSkills { get; set; } = new();

        // 3. For the Dropdowns / Checkboxes:
        // Holds the master list of all services to choose from
        public IEnumerable<SelectListItem>? AvailableServices { get; set; }
    }

    // Helper class for the inner list of existing skills
    public class TrainerSkillItem
    {
        public int Id { get; set; }          // The TrainerSkill ID (needed to delete)
        public int ServiceId { get; set; }   // The Service ID
        public string ServiceName { get; set; }
    }
}