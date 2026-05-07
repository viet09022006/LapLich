using CalendarApp.API.DTOs;

namespace CalendarApp.API.Services
{
    public interface IAuthService
    {
        Task<(bool IsSuccess, string Message, object? Data)> RegisterAsync(RegisterRequest request);
        Task<(bool IsSuccess, string Message, object? Data)> LoginAsync(LoginRequest request);
    }
}
