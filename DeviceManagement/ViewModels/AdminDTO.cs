using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string CellphoneNumber { get; set; }
        public string Gender { get; set; }
        public string CompanyName { get; set; }
        public string StudentNumber { get; set; }
        public string BusinessNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string SAIdNumber { get; set; }
        public decimal MonthlyEarnings { get; set; }
        public int OwnedLaptopsCount { get; set; }
        public int NotificationsCount { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }

    // ViewModel for editing user (only 5 editable fields)
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        [Phone]
        public string CellphoneNumber { get; set; }

        [MaxLength(50)]
        public string Gender { get; set; }

        [MaxLength(100)]
        public string CompanyName { get; set; }
    }
}
