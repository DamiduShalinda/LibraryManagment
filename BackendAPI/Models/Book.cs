namespace BackendAPI.Models
{
    public class Book
    {
        public int Id { get; set; }
        public required string BookName { get; set; }
        public required string ISBN { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }

        public DateTime AdddedAt { get; set; }

        public Book()
        {
            this.AdddedAt = DateTime.Now;
        }
    }
}
