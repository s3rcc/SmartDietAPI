using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.AuthDTOs
{
    public class ChangePasswordRequest
    {	public  string? OldPassword { get; set; }
    	public  string? NewPassword { get; set; }
    }
}
