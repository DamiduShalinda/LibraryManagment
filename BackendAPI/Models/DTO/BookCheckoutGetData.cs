namespace BackendAPI.Models.DTO
{
    public record BookCheckoutGetData(List<string> BookNames);

    public class BookCheckoutBasicData(int id, string authorname, string bookName)
    {
        public int Id { get; set; } = id;
        public string Authorname { get; set; } = authorname;
        public string BookName { get; set; } = bookName;
    };

    public record BookCheckOutRequest(List<int> BookList , DateTime CheckOutDate);
}
