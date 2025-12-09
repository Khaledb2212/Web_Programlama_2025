using System.ComponentModel.DataAnnotations;
using Web_API.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
namespace Web_API.Models
{
    [Table("People")]
    public class Person
    {
        [Key]
        public int PersonID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string? Firstname { get; set; } 

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string? Lastname { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        [MaxLength(15, ErrorMessage = "Phone number is too long.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)] // Hides input in HTML forms
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string? Password { get; set; }

        
        public byte Role { get; set; }
        //i will do it later as 0 admin, 1 trainer, 2 member
        public Person()
        {

        }

        // Your existing constructor
        public Person(int id, string firstname, string lastname, string email, string phone, string username, string password, byte rule)
        {
            this.PersonID = id;
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.Email = email;
            this.Phone = phone;
            this.Username = username;
            this.Password = password;
            this.Role = rule;
        }
    }


}
