using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BackendAPI.Models
{
    public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser> (options)
    {
        
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Book>()
               .HasOne(b => b.Author)
               .WithMany(a => a.Books)
               .HasForeignKey(b => b.AuthorId);
        }
    }
}
