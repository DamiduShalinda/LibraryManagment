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

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController(ApplicationDbContext context, UserManager<ApplicationUser> userManager , JwtHelper jwtHelper) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly JwtHelper _jwtHelper = jwtHelper;


        [HttpGet("usernames")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserNames()
        {
            var userList = await _userManager.Users.ToListAsync();
            return userList.Select(_ => _.Name ?? "").ToList();
        }

        [HttpGet("booknames")]
        public async Task<ActionResult<IEnumerable<string>>> GetBooksName()
        {
            var userList = await _context.Books.ToListAsync();
            return userList.Select(_ => _.BookName).ToList();
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

                List<Book> bookList = [];
                foreach (var id in request.BookList)
                {
                    var book = await _context.Books.FindAsync(id);
                    if (book == null)
                        return NotFound();
                    else
                        bookList.Add(book);
                }

                BorrowedBooks borrowedBooks = new()
                {
                    Books = bookList,
                    BorrowedDate = DateTime.Now,
                    ReturnedDate = request.CheckOutDate,
                    ApplicationUserId = userIdClaim
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
    }
}
