using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models
{
    public class Rezervation
    {
        public long Id { get; set; }
        public string? RezervationReason { get; set; }
        public DateTime RezervationDate { get; set; }
        public string MembersId { get; set; } = "";
        [ForeignKey(nameof(MembersId))]
        public Member? Member { get; set; }
        public int TableNumber { get; set; }

    }
}