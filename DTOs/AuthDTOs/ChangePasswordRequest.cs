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
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,16}$", ErrorMessage = "Password must be at least 8 characters, 1 uppercase, 1 lowercase, 1 number and 1 special character")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Required new password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,16}$", ErrorMessage = "Password must be at least 8 characters, 1 uppercase, 1 lowercase, 1 number and 1 special character")]
        public string NewPassword { get; set; }
    }
}
