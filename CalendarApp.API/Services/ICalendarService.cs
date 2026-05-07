using CalendarApp.API.DTOs;

namespace CalendarApp.API.Services
{
    public interface ICalendarService
    {
        Task<IEnumerable<object>> GetUsersAsync();
        Task<(bool IsSuccess, IEnumerable<object>? Data, string Message)> GetAppointmentsAsync(string userId);
        Task<(int StatusCode, object? Data, string? ErrorMessage)> AddAppointmentAsync(string userId, CreateAppointmentRequest request);
        Task<(bool IsSuccess, string Message)> JoinGroupMeetingAsync(string meetingId, string userId);
    }
}
