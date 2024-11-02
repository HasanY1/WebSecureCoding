using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using PostService.Data;
using PostService.models;
using PostService.Models;
using PostService.Models.Auth;
using PostService.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PostService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(ApplicationDbContext context, IConfiguration configuration, Utilities.EmailService emailService) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly string _secretKey = configuration["Jwt:SecretKey"] ?? throw new ArgumentNullException("Jwt:SecretKey", "The JWT secret key is not configured.");
        private readonly Utilities.EmailService ? _emailService=emailService;








        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest("User with this email does not exist.");
            }

            // Generate password reset token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var resetUrl = Url.Action("ResetPassword", "Auth", new { token = tokenString }, Request.Scheme);
            var emailMessage = $"To reset your password, click this link: <a href='{resetUrl}'>Reset Password</a>";
            await _emailService.SendEmailAsync(user.Email, "Reset your password", emailMessage);

            return Ok("Password reset link has been sent to your email.");
        }





        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] models.ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("New password is required.");
            }

            // Retrieve the token from the Authorization header
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Bearer token is missing or invalid.");
            }

            // Extract the token by removing the "Bearer " prefix
            var token = authHeader.Substring("Bearer ".Length).Trim();

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);

                var username = claimsPrincipal.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest("Invalid token.");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return BadRequest("Invalid token.");
                }

                // Hash and update the password
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                return Ok("Password has been reset successfully.");
            }
            catch
            {
                return BadRequest("Invalid token.");
            }
        }















        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Models.Auth.RegisterRequest request)
        {
            // Sanitize request
            bool isValid = SanitizationHelper.SanitizeRequest(request);
            if (!isValid)
            {
                return BadRequest("All fields are required.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("User already exists.");
            }

            // Create a new User entity
            var user = new User
            {
                FullName = request.FullName,
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = request.Email,
                City = request.City,
                Country = request.Country,
                Phone = request.Phone,
                IsActive = false // Set IsActive to false initially
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var token = GenerateVerificationToken(user); // Create a method to generate the token
            var verificationUrl = Url.Action("VerifyToken", "Auth", new { token }, Request.Scheme); // Generate the verification URL
            var emailMessage = $"Please verify your email by clicking this link: <a href='{verificationUrl}'>Verify Email</a>";
            await _emailService.SendEmailAsync(user.Email, "Verify your email", emailMessage);

            return Ok("User registered successfully. Please check your email to activate your account.");
        }

        // Method to generate a verification token
        private string GenerateVerificationToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Models.Auth.LoginRequest request)
        {
            if (!ModelState.IsValid) // Check if the model state is valid
            {
                return BadRequest(ModelState); // Return validation errors
            }

            // Fetch user from the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // Validate user and password
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Invalid username or password.");
            }


            if ((user.IsActive != true))
            {
                return Unauthorized("Please Verify Your Account.");
            }


            // Check if the username is null before creating the claim
            if (string.IsNullOrEmpty(user.Username))
            {
                return BadRequest("User username is not valid.");
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, user.Username)]),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Return token along with user's full name and email
            return Ok(new
            {
                Token = tokenString,
                user.FullName,
                user.Email
            });
        }












        [HttpGet("verify")]
        public async Task<IActionResult> VerifyToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Validate the token
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);

                // Get the username from the claims and check for null
                var username = claimsPrincipal.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest("Invalid token.");
                }

                // Find the user in the database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return BadRequest("Invalid token.");
                }

                // Check if the user is already active
                if (user.IsActive == true) // Check for active status
                {
                    return BadRequest("Account is already active.");
                }

                // Set IsActive to true
                user.IsActive = true;
                await _context.SaveChangesAsync();

                return Ok("Your account has been activated successfully.");
            }
            catch
            {
                return BadRequest("Invalid token.");
            }
        }



    }
}