namespace BackendAPI.Models
{
    public class Books
    {
        public int BookId { get; set; }
        public required string BookName { get; set; }
        public required string ISBN { get; set; }

        public DateTime AdddedAt { get; set; }

        public Books()
        {
            this.AdddedAt = DateTime.Now;
        }
    }
}
