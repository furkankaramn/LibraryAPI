using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
    public class BookSubCategory
    {
        
        public short SubCategoriesId { get; set; }

        [ForeignKey(nameof(SubCategoriesId))]
        public SubCategory? SubCategory { get; set; }

        public int BooksId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(BooksId))]
        public Book? Book { get; set; }
    }
}
