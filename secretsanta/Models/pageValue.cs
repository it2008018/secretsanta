using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace secretsanta.Models
{
    public class pageValue
    {
        [Required(ErrorMessage ="Please enter address")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Please enter Name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        public string Password { get; set; }
        public string GeneratedName { get; set; }
        public string UpdateAddress { get; set; }
    }
}
