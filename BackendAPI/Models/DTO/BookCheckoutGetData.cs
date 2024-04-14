namespace BackendAPI.Models.DTO
{
    public record BookCheckoutGetData(List<string> BookNames);

    public class BookCheckoutBasicData(int id, string authorName, string bookName)
    {
        public int Id { get; set; } = id;
        public string Authorname { get; set; } = authorName;
        public string BookName { get; set; } = bookName;
    };

    public record BookCheckOutRequest(List<int> BookList , DateTime CheckOutDate);

    public record BookCheckOutListItem(int Id , string? UserName , DateTime RequestedDate);
}
