using System.ComponentModel.DataAnnotations;

namespace PostService.Models.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Full name is required.")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string ? Password { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string ? Email { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string ? City { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string ? Country { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string ? Phone { get; set; }
    }
}
