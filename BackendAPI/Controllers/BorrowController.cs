using BackendAPI.Models;
using BackendAPI.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;


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
    }
}
