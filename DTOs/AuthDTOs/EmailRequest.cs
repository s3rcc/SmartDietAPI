using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.AuthDTOs
{
    public class EmailRequest
    {
        [Required(ErrorMessage = "Required Email")]
        [EmailAddress(ErrorMessage = "Email not valid")]
        public string Email { get; set; }
    }
}
