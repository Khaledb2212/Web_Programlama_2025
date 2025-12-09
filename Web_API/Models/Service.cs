using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web_API.Models
{
    public class Service
    {
        [Key]
        public int ServiceID { get; set; }

        [Required(ErrorMessage = "Service Name is required.")]
        [StringLength(100, ErrorMessage = "Service Name cannot exceed 100 characters.")]
        [Column("Service Name")] // Maps to "Service Name" (with space) in SQL
        public string? ServiceName { get; set; }

        // Diagram says 'bigint', so we use 'long'. 
        // Note: For money, 'decimal' is usually better, but 'long' works for integers.
        [Column("Fees Per Hour")]
        public int FeesPerHour { get; set; }

        public string? Details { get; set; }

        // Constructor
        public Service() { }
    }
}
