using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.AuthDTOs
{
    public class ResetPasswordRequest
    {
          [Required(ErrorMessage = "Required Email")]
        [EmailAddress(ErrorMessage = "Email not valid")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
		//[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,16}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*\d)[a-z\d]{8,16}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ thường, 1 số")]
		public string Password { get; set; }
    }
}
