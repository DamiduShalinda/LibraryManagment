using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;


namespace BackendAPI.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;

        public virtual ICollection<Book> Books { get; set; } = [];
    }
}
