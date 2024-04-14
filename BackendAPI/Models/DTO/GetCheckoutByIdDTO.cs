namespace BackendAPI.Models.DTO
{
    public record GetCheckoutByIdDTO(int Id , DateTime BorrowedDate , bool IsApproved , string RejectedReason , List<GetCheckoutByIdBooksDTO> Books , GetCheckoutByIdUserDTO User  );

    public record GetCheckoutByIdBooksDTO(int Id, string BookName, string ISBN, string Status);

    public record GetCheckoutByIdUserDTO(string UserId , string? Name , string? Email);
}
