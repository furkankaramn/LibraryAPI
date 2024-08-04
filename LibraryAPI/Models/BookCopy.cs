using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
    public class BookCopy
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }

        [Required]
        public string Barcode { get; set; } = "";

        public bool IsAvailable { get; set; } = true;
        
        public List<Borrow>? Borrows { get; set; }
    }
}
