using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BackendAPI.Models
{
    public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser> (options)
    {
        
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
    }
}
