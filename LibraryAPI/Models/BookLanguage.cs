using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
    public class BookLanguage
    {
        

        [Required]
        public string LanguagesCode { get; set; } = "";

        [ForeignKey(nameof(LanguagesCode))]
        public Language? Language { get; set; }

        public int BooksId { get; set; }

        [ForeignKey(nameof(BooksId))]
        public Book? Book { get; set; }
    }
}
