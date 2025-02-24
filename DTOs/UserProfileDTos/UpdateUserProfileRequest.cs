using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserProfileDTos
{
    public class UpdateUserProfileRequest
    {
        public string? FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? TimeZone { get; set; }
        public string? PreferredLanguage { get; set; } 

        public bool EnableNotifications { get; set; } 

        public bool EnableEmailNotifications { get; set; } 

        public bool EnablePushNotifications { get; set; }

    }
}
