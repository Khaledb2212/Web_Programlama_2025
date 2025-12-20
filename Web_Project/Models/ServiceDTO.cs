using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Web_Project.Models
{
    public class ServiceDTO
    {
        [JsonPropertyName("serviceID")]
        public int ServiceID { get; set; }

        [Required]
        [Display(Name = "Service Name")]
        [JsonPropertyName("serviceName")]
        public string? ServiceName { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Fees must be between 0 and 1000")]
        [Display(Name = "Fees Per Hour")]
        [JsonPropertyName("feesPerHour")]
        public decimal FeesPerHour { get; set; }

        [Required]
        [Display(Name = "Details")]
        [JsonPropertyName("details")]
        public string? Details { get; set; }
    }
}