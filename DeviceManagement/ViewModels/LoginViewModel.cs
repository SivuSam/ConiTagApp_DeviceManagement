using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.ViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
    public class ApiUserDto
    {
        public string username { get; set; }
        public string password { get; set; }
        public string full_name { get; set; }
        public string email { get; set; }
        public int role_id { get; set; }
        public string status { get; set; }
        public string staff_number { get; set; }
        public string student_number { get; set; }
        public string sa_id { get; set; }
    }


    public class RegisterViewModel 
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        [Phone]
        [Display(Name = "Cellphone Number")]
        public string CellphoneNumber { get; set; }

        [MaxLength(50)]
        public string Gender { get; set; } // You can later populate from dropdown

        [MaxLength(100)]
        [Display(Name = "Company Name (if applicable)")]
        public string CompanyName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Student Number (if student)")]
        public string StudentNumber { get; set; }

        [MaxLength(50)]
        [Display(Name = "Business Number (if business)")]
        public string BusinessNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "SA ID Number must be exactly 13 digits.")]
        [Display(Name = "South African ID Number")]
        public string SAIdNumber { get; set; }

  
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }


}
