using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserProfileDTos
{
    public class UserProfileDTO
    {
        public string FullName { get; set; }
        public IFormFile ProfilePicture { get; set; }
        public string TimeZone { get; set; } = "UTC";
        public string PreferredLanguage { get; set; } = "en";
        public bool EnableNotifications { get; set; } = true;
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnablePushNotifications { get; set; } = false;
    }
}
