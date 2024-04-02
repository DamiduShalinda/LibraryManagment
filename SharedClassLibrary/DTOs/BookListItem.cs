using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedClassLibrary.DTOs
{
    public record BookListItem(int Id , string BookName, string ISBN , string AuthorName);
}
