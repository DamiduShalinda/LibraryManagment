namespace BackendAPI.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string BookName { get; set; }
        public string ISBN { get; set; }
        public int AuthorId { get; set; }
        public virtual Author Author { get; set; }

        public DateTime AdddedAt { get; set; } = DateTime.Now;

        public Book()
        {
        }

        public Book(string bookName, string iSBN, Author author)
        {
            BookName = bookName;
            ISBN = iSBN;
            Author = author;
        }
    }
}
