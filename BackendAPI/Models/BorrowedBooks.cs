using System;

namespace BackendAPI.Models
{
    public class BorrowedBooks
    {
        public int Id { get; set; }

        public DateTime BorrowedDate { get; set; } = DateTime.Now;

        public DateTime ReturnedDate { get; set; }

        public bool IsApproved { get; set; } = false;

        public bool IsCompleted { get; set; } = false;

        public DateTime CompletedDate { get; set; }

        public string RejectedReason { get; set; } = "";
        public string Remarks { get; set; } = "";

        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
        public  string ApplicationUserId { get; set; }
        public virtual  ApplicationUser ApplicationUser { get; set; }

    }


}
