using Microsoft.AspNetCore.Identity;

namespace BackendAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }

        public virtual ICollection<BorrowedBooks> BorrowedBooks { get; set;} = new List<BorrowedBooks>();
    }
}
