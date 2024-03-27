namespace BackendAPI.Models
{
    public class BorrowedBooks
    {
        public int BorrowedBookId { get; set; }

        public DateTime BorrowedDate { get; set; }

        public DateTime? ReturnedDate { get; set; }
    }
}
