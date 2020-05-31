using Microsoft.AspNetCore.Identity;

namespace ReactOnlineActivity.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string PhotoUrl { get; set; }
        [PersonalData]
        public UserStatistics Statistics { get; set; }
    }
}