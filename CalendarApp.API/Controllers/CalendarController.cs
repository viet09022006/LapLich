using Microsoft.AspNetCore.Mvc;
using CalendarApp.API.Services;
using CalendarApp.API.DTOs;

namespace CalendarApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        // GET: api/calendar/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _calendarService.GetUsersAsync();
            return Ok(users);
        }

        // GET: api/calendar/{userId}/appointments
        [HttpGet("{userId}/appointments")]
        public async Task<IActionResult> GetAppointments(string userId)
        {
            var result = await _calendarService.GetAppointmentsAsync(userId);
            if (!result.IsSuccess) return NotFound(result.Message);
            return Ok(result.Data);
        }

        // POST: api/calendar/{userId}/appointments
        [HttpPost("{userId}/appointments")]
        public async Task<IActionResult> AddAppointment(string userId, [FromBody] CreateAppointmentRequest request)
        {
            var result = await _calendarService.AddAppointmentAsync(userId, request);

            if (result.StatusCode == 404) return NotFound(result.ErrorMessage);
            if (result.StatusCode == 400) return BadRequest(result.ErrorMessage);
            if (result.StatusCode == 409) return StatusCode(409, result.Data);
            
            return Ok(result.Data);
        }

        // POST: api/calendar/groupmeetings/{meetingId}/join/{userId}
        [HttpPost("groupmeetings/{meetingId}/join/{userId}")]
        public async Task<IActionResult> JoinGroupMeeting(string meetingId, string userId)
        {
            var result = await _calendarService.JoinGroupMeetingAsync(meetingId, userId);
            if (!result.IsSuccess) return NotFound();

            return Ok(new { message = result.Message });
        }
    }
}
