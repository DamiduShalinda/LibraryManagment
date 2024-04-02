using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendAPI.Models;
using SharedClassLibrary.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using BackendAPI.Models.DTO;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;


        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDataWithAuthor>>> GetBooks()
        {
            return await _context.Books
                .Select(_ => new BookDataWithAuthor(_.Id , _.BookName , _.ISBN , _.Author.AuthorName))
                .ToListAsync();
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBooks(int id)
        {
            var books = await _context.Books.FindAsync(id);

            if (books == null)
            {
                return NotFound();
            }

            return books;
        }

        // PUT: api/Books/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooks(int id, Book books)
        {
            if (id != books.Id)
            {
                return BadRequest();
            }

            _context.Entry(books).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BooksExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Books
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Book>> PostBooks(BookDTO bookDTO)
        {
            var book = new Book
            {
                AdddedAt = DateTime.Now,
                AuthorId = bookDTO.AuthorId,
                BookName = bookDTO.BookName,
                ISBN = bookDTO.ISBN,
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBooks", new { id = book.Id }, book);
        }


        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooks(int id)
        {
            var books = await _context.Books.FindAsync(id);
            if (books == null)
            {
                return NotFound();
            }

            _context.Books.Remove(books);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BooksExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
