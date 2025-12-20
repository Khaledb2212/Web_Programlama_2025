using System.Text.Json.Serialization;

namespace Web_Project.Models
{
    public class ApiTrainer
    {
        [JsonPropertyName("trainerID")]
        public int TrainerID { get; set; }

        [JsonPropertyName("personID")]
        public int PersonID { get; set; }

        // Matches the "person": { ... } object in JSON
        [JsonPropertyName("person")]
        public ApiPerson? Person { get; set; }

        [JsonPropertyName("expertiseAreas")]
        public string? ExpertiseAreas { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        // Matches the "skills": [ ... ] array in JSON
        [JsonPropertyName("skills")]
        public List<ApiTrainerSkill>? Skills { get; set; }
    }

    // 2. Catches the nested "person" object
    public class ApiPerson
    {
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    // 3. Catches the nested "skills" items
    public class ApiTrainerSkill
    {
        [JsonPropertyName("id")]
        public int TrainerSkillID { get; set; }

        [JsonPropertyName("serviceID")]
        public int ServiceID { get; set; }

        [JsonPropertyName("service")]
        public ServiceDTO? Service { get; set; } // We can reuse your existing ServiceDTO here
    }
}
