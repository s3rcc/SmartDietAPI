using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.AuthDTOs
{
    public class ConfirmOtpRequest
    {
        [Required(ErrorMessage = "Required email")]
        [EmailAddress(ErrorMessage = "Email not valid")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Required OTP")]
        public string OTP { get; set; }
    }
}
