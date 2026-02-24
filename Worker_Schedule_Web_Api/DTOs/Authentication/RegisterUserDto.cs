using System.ComponentModel.DataAnnotations;

namespace Worker_Schedule_Web_Api.DTOs.Authentication
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Internal Number is required!")]
        [Range(1, 10000000)]
        public int WorkerInternalNumber { get; set; }

        [Required(ErrorMessage = "First name is required!")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required!")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employment Percentage is required!")]
        [Range(1, 100)]
        public int EmploymentPercentage { get; set; }

        public int StoreId { get; set; } = 16614;
        public Guid PositionId { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required!")]
        public string Password { get; set; } = string.Empty;
    }
}
