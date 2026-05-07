using Microsoft.EntityFrameworkCore;
using CalendarApp.API.Data;
using CalendarApp.API.Entities;
using CalendarApp.API.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace CalendarApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsSuccess, string Message, object? Data)> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return (false, "Username already exists.", null);
            }

            var user = new User
            {
                Name = request.Name,
                Username = request.Username,
                PasswordHash = HashPassword(request.Password)
            };

            var calendar = new Calendar { UserID = user.UserID };

            _context.Users.Add(user);
            _context.Calendars.Add(calendar);
            
            await _context.SaveChangesAsync();

            return (true, "Registration successful", null);
        }

        public async Task<(bool IsSuccess, string Message, object? Data)> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return (false, "Invalid username or password.", null);
            }

            bool isPasswordValid = user.PasswordHash == request.Password || user.PasswordHash == HashPassword(request.Password);

            if (!isPasswordValid)
            {
                return (false, "Invalid username or password.", null);
            }

            return (true, "Login successful", new { userID = user.UserID, name = user.Name });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
