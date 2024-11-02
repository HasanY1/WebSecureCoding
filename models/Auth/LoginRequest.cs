using System.ComponentModel.DataAnnotations;

namespace PostService.Models.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "username is required.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }


    }
}
