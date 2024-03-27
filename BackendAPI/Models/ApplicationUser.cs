using Microsoft.AspNetCore.Identity;

namespace BackendAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
