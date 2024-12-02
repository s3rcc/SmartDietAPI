using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.AuthDTOs
{
    public class TokenGoogleRequest
    {
        [Required(ErrorMessage = "Required Token")]
        public string token { get; set; }
    }
}
