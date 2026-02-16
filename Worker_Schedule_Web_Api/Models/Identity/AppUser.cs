using Microsoft.AspNetCore.Identity;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Models.Identity
{
    public class AppUser : IdentityUser
    {
        public Worker? Worker { get; set; }
    }
}
