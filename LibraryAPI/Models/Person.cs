using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public long IdNumber { get; set; }
        public string Name { get; set; } = "";
        public string? MiddleName { get; set; }
        public string? FamilyName { get; set; }
        public string Address { get; set; } = "";
        public bool Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime RegisterDate { get; set; }
        public byte Status { get; set; }
        [NotMapped]
        public string? Password { get; set; }
        [NotMapped]
        [Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; }

        public bool IsActive { get; set; } = true;
    }
    public class Member
    {
        [Key]
        
        public string Id { get; set; } = "";

        [ForeignKey(nameof(Id))]
        public ApplicationUser? ApplicationUser { get; set; }

        public byte EducationalDegree { get; set; }

        public List<Borrow>? Borrows { get; set; }

        public decimal TotalPenaltyFee { get; set; }
    }
    public class Employee
    {
        [Key]
        public string Id { get; set; } = "";

        [ForeignKey(nameof(Id))]
        public ApplicationUser? ApplicationUser { get; set; }

        public string Title { get; set; } = "";
        public float Salary { get; set; }
        public string Department { get; set; } = "";
        public string? Shift { get; set; }

        [JsonIgnore]
        public List<Borrow>? Borrows { get; set; }
    }
}


