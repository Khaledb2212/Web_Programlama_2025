using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web_API.Models
{
    public class Member
    {
        [Key]
        public int MemberID { get; set; }

        public int PersonID { get; set; }
        [ForeignKey("PersonID")]
        public Person? person { get; set; } 

    }
}
