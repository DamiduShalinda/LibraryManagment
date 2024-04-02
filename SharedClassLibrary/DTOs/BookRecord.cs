using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedClassLibrary.DTOs
{
    public class BookDTO()
    {
        [Required]
        public string BookName { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Range(1, int.MaxValue)]
        public int AuthorId { get; set; }
    };
}
