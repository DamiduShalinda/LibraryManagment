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
        public string BookDescription { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string AuthorName { get; set; }
    };
}
