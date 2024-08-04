using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models
{
    public class Category
    {
        public short Id { get; set; }
        [Required]
        [StringLength(800)]
        [Column(TypeName = "varchar(800)")]
        public string Name { get; set; } = "";
        public List<SubCategory>? SubCategories { get; set; }
    }
}
