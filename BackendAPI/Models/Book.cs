namespace BackendAPI.Models
{
    public enum BookStatus
    {
        Pending,
        Available,
        NotAvailable
    }
    public class Book
    {
        public int Id { get; set; }
        public string BookName { get; set; }
        public string ISBN { get; set; }
        public int AuthorId { get; set; }

        public BookStatus Status { get; set; } = BookStatus.Available;

        public string BookDescription { get; set; }
        public virtual Author Author { get; set; }
        public string BookCoverImagePath { get; set; }

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

        public void SaveCoverImage(byte[] imageBytes , string folderPath , string FileName)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filePath = Path.Combine(folderPath, FileName);
            File.WriteAllBytes(filePath, imageBytes);

            BookCoverImagePath = filePath;
        }

        public byte[] GetCoverImage()
        {
            if (File.Exists(BookCoverImagePath))
            {
                return File.ReadAllBytes(BookCoverImagePath);
            } else
            {
                return null;
            }
        }
    }
}
