using System.ComponentModel.DataAnnotations;

namespace Worker_Schedule_Web_Api.DTOs.Authentication
{
    public class LoginUserDto
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required!")]
        public string Password { get; set; } = string.Empty;
    }
}
