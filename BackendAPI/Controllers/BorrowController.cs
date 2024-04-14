using BackendAPI.Models;
using BackendAPI.Models.DTO;
using BackendAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Reflection.Metadata.BlobBuilder;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController(ApplicationDbContext context, UserManager<ApplicationUser> userManager , JwtHelper jwtHelper) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly JwtHelper _jwtHelper = jwtHelper;


        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookCheckOutListItem>>> GetAllBookCheckouts()
        {
            try
            {
                var bookCheckOutList = await _context.BorrowedBooks.Where(_ => _.IsApproved == false).ToListAsync();
                if (bookCheckOutList.Count == 0)
                    return NoContent();
               List<BookCheckOutListItem> itemList = bookCheckOutList.Select(_ => new BookCheckOutListItem(_.Id, _.ApplicationUserId, _.BorrowedDate)).ToList();
                return Ok(itemList);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetCheckoutByIdDTO>> GetCheckOutBookById(int Id)
        {
            try
            {
                var item = await _context.BorrowedBooks
                        .Include(_ => _.Books)
                        .Include(_ => _.ApplicationUser)
                        .FirstOrDefaultAsync(_ => _.Id == Id);
                if (item == null)
                    return NoContent();
                GetCheckoutByIdDTO response = new(
                    Id: item.Id,
                    BorrowedDate: item.BorrowedDate,
                    IsApproved: item.IsApproved,
                    RejectedReason: item.RejectedReason,
                    Books: item.Books.Select(_ => new GetCheckoutByIdBooksDTO(_.Id, _.BookName, _.ISBN, _.Status.ToString())).ToList(),
                    User: new GetCheckoutByIdUserDTO(item.ApplicationUser.Id, item.ApplicationUser.Name, item.ApplicationUser.Email)
                    );

                return Ok(response);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("get-checkout-books-data")]
        public async Task<ActionResult<IEnumerable<BookCheckoutBasicData>>> GetCheckoutBooksDetails(List<string> data)
        {
            try
            {
                var books = await _context.Books
                    .Where(_ => data.Contains(_.BookName))
                    .Select(_ => new BookCheckoutBasicData(_.Id, _.Author.AuthorName, _.BookName))
                    .ToListAsync();
                if (books.Count != data.Count)
                {
                    var notFoundBooks = data.Except(books.Select(b => b.BookName));
                    return BadRequest($"No Matching Books for: {string.Join(", ", notFoundBooks)}");
                }
                return Ok(books);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("checkout")]
        [Authorize(Roles ="User")]
        public async Task<ActionResult<string>> CheckOutBooks(BookCheckOutRequest request)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized();
                var previousBookRequest = await _context.BorrowedBooks.FirstOrDefaultAsync(_ => _.ApplicationUser.Id == userIdClaim);
                if (previousBookRequest != null && previousBookRequest.IsApproved == false)
                    return Conflict("Already Have a pending request under this user");

                List<Book> bookList = [];
                foreach (var id in request.BookList)
                {
                    var book = await _context.Books.FindAsync(id);
                    if (book == null)
                        return NotFound();
                    else
                        bookList.Add(book);
                }

                var user = await _userManager.Users.FirstOrDefaultAsync(_ => _.Id == userIdClaim);
                if (user == null)
                    return NotFound();

                BorrowedBooks borrowedBooks = new()
                {
                    Books = bookList,
                    ReturnedDate = request.CheckOutDate,
                    ApplicationUser = user
                };
                _context.BorrowedBooks.Add(borrowedBooks);
                await _context.SaveChangesAsync();
                return Ok($"Send To Approve {userIdClaim}");
            } catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(errorMessage);
            }
        }

        [HttpGet("approve/{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<string>> ApprovingCheckOutRequest(int id , string? RejectedReason)
        {
            try
            {
                var BorrowedBooksRecord = await _context.BorrowedBooks.FindAsync(id);
                if (BorrowedBooksRecord == null)
                    return NotFound($"No Record Founder under id:{id}");
                if (RejectedReason == null)
                    BorrowedBooksRecord.IsApproved = true;
                else
                    BorrowedBooksRecord.RejectedReason = RejectedReason;
                await _context.SaveChangesAsync();
                string status = RejectedReason != null ? "Rejected" : "Approved";
                return Ok($"Request under id:{id} is {status}");
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteCheckoutRequest(int id)
        {
            try
            {
                var deletingRequest = await _context.BorrowedBooks.FindAsync(id);
                if (deletingRequest == null)
                    return NotFound();
                _context.BorrowedBooks.Remove(deletingRequest);
                await _context.SaveChangesAsync();
                return Ok("Deleted the checkout Request");
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
