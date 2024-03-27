namespace BackendAPI.Models
{
    public class BorrowedBooks
    {
        public int Id { get; set; }

        public DateTime BorrowedDate { get; set; }

        public DateTime? ReturnedDate { get; set; }

        public int BookId { get; set; }
        public required Book Book { get; set; }
        public int ApplicationUserId { get; set; }
        public required ApplicationUser ApplicationUser { get; set; }
        public BorrowedBooks()
        {
            this.BorrowedDate = DateTime.Now;
        }
    }
}
