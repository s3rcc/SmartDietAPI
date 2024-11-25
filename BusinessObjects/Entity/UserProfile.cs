using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class UserProfile : BaseEntity
    {
        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        public string FullName { get; set; }
        public string ProfilePicture { get; set; }

        [Required]
        public string TimeZone { get; set; } = "UTC";

        [StringLength(10)]
        public string PreferredLanguage { get; set; } = "en";

        public bool EnableNotifications { get; set; } = true;

        public bool EnableEmailNotifications { get; set; } = true;

        public bool EnablePushNotifications { get; set; } = false;


    }
}
