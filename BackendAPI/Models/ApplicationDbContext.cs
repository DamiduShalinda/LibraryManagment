using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BackendAPI.Models
{
    public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser> (options)
    {
        
    }
}
