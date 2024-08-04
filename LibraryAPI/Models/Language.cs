using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models
{
    public class Language
    {
        [Key]
        [Required]
        [StringLength(3, MinimumLength = 3)]
        [Column(TypeName = "char(3)")]
        public string Code { get; set; } = "";
        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = "";
        public List<BookLanguage>? BookLanguages { get; set; }
    }
}