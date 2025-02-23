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
		[RegularExpression(@"^(?=.*[a-z])(?=.*\d)[a-z\d]{8,16}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ thường, 1 số")]
		public string OldPassword { get; set; }
        [Required(ErrorMessage = "Required new password")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*\d)[a-z\d]{8,16}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ thường, 1 số")]
		public string NewPassword { get; set; }
    }
}
