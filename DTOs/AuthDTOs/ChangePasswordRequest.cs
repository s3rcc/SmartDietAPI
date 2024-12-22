using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.AuthDTOs
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Required old password")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Password must be 6 number.")] public string OldPassword { get; set; }
        [Required(ErrorMessage = "Required new password")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Password must be 6 number.")] public string NewPassword { get; set; }
    }
}
