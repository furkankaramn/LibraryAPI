using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
    public class Borrow
    {
        public long Id { get; set; }

        public int BookCopiesId { get; set; }

        [ForeignKey(nameof(BookCopiesId))]
        [JsonIgnore]
        public BookCopy? BookCopy { get; set; }

        public string EmployeesId { get; set; } = "";
        [JsonIgnore]
        [ForeignKey(nameof(EmployeesId))]
        public Employee? Employee { get; set; }

        public string MembersId { get; set; } = "";
        [JsonIgnore]
        [ForeignKey(nameof(MembersId))]
        public Member? Member { get; set; }

        public DateTime PickUpDate { get; set; }
        public DateTime DeliveryDate { get; set; }

        public DateTime? TakenBackDate { get; set; }

        

        public int? DelayTime { get; set; }

        public decimal? PenaltyFee { get; set; } 
    }
}
