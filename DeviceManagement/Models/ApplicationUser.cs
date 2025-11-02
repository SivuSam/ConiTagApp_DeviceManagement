using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Existing properties
        [Precision(18, 2)]
        public decimal MonthlyEarnings { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        [Phone]
        public string CellphoneNumber { get; set; }

        [EmailAddress]
        public override string Email { get; set; }

        [MaxLength(50)]
        public string Gender { get; set; } // Male, Female, Other

        [MaxLength(100)]
        public string CompanyName { get; set; } // For business users

        [MaxLength(50)]
        public string? StudentNumber { get; set; } // For students

        [MaxLength(50)]
        public string BusinessNumber { get; set; } // For registered businesses

        public System.DateTime? DateOfBirth { get; set; }

        [Required]
        [MaxLength(13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "SA ID Number must be exactly 13 digits.")]
        public string SAIdNumber { get; set; }

        // Navigation property for notifications
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
