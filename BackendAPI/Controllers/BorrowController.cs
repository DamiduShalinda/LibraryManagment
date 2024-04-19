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


        [HttpGet]
        [Authorize]
        //if role is user it returns user's request and if role is admin it returns all requests
        public async Task<ActionResult<IEnumerable<BookCheckOutListItem>>> GetAllBookCheckoutsByAdmin([FromQuery] bool? IsApproved)
        {
            try
            {

                var userRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userRole == null || userIdClaim == null) 
                    return Unauthorized();
                IQueryable<BorrowedBooks> query = _context.BorrowedBooks.Include(_ => _.ApplicationUser);
                if (userRole == "User")
                    query = query.Where(_ => _.ApplicationUser.Id == userIdClaim);
                if (IsApproved.HasValue)
                    query =    query.Where(_ => _.IsApproved == IsApproved.Value);
                var bookCheckOutList = await query.ToListAsync();
                if (bookCheckOutList.Count == 0)
                    return NoContent();
               List<BookCheckOutListItem> itemList = bookCheckOutList.Select(_ => new BookCheckOutListItem(_.Id, _.ApplicationUser.Name, _.BorrowedDate , _.IsApproved)).ToList();
                return Ok(itemList);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles ="Admin")]
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
        //Add Checkout request
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
                        book.Status = BookStatus.Pending;
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
        //Approve or Reject Checkout Request
        public async Task<ActionResult<string>> ApprovingCheckOutRequest(int id , string? RejectedReason)
        {
            try
            {
                var BorrowedBooksRecord = await _context.BorrowedBooks.FindAsync(id);
                if (BorrowedBooksRecord == null)
                    return NotFound($"No Record Founder under id:{id}");
                if (RejectedReason == null)
                {
                    BorrowedBooksRecord.IsApproved = true;
                    foreach (var book in BorrowedBooksRecord.Books)
                        book.Status = BookStatus.NotAvailable;
                }
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

        [HttpGet("complete/{id}")]
        [Authorize(Roles = "Admin")]
        //Complete Checkout request after approved it
        public async Task<ActionResult<string>> CompleteCheckoutRequest(int id , string? Remarks)
        {
            try
            {
                var BorrowedBooksRecord = await _context.BorrowedBooks.FindAsync(id);
                if (BorrowedBooksRecord == null)
                    return NotFound($"Not any Records found under id:{id}");
                if (BorrowedBooksRecord.IsApproved == false)
                    return NotFound($"Not any Appoved Records found under id:{id}");
                BorrowedBooksRecord.IsApproved= true;
                BorrowedBooksRecord.CompletedDate = DateTime.Now;
                if (Remarks != null)
                    BorrowedBooksRecord.Remarks = Remarks;
                foreach (var books in BorrowedBooksRecord.Books)
                {
                    var dbBook = await _context.Books.FindAsync(books.Id);
                    if (dbBook == null)
                        continue;
                    dbBook.Status = BookStatus.Available;
                }
                await _context.SaveChangesAsync();
                return Ok($"Request under id:{id} is completed");
            } catch(Exception ex)
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
                foreach (var book in deletingRequest.Books)
                    book.Status = BookStatus.Available;
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
