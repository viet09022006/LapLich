using Microsoft.EntityFrameworkCore;
using CalendarApp.API.Data;
using CalendarApp.API.Entities;
using CalendarApp.API.DTOs;

namespace CalendarApp.API.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly AppDbContext _context;

        public CalendarService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetUsersAsync()
        {
            return await _context.Users.Select(u => new { u.UserID, u.Name }).ToListAsync();
        }

        public async Task<(bool IsSuccess, IEnumerable<object>? Data, string Message)> GetAppointmentsAsync(string userId)
        {
            var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.UserID == userId);
            if (calendar == null) return (false, null, "User calendar not found.");

            var appointments = await _context.Appointments
                .Where(a => a.CalendarID == calendar.CalendarID || 
                           (a is GroupMeeting && ((GroupMeeting)a).Participants.Any(p => p.UserID == userId)))
                .Select(a => new {
                    a.AppointmentID,
                    a.Name,
                    a.Location,
                    a.Start,
                    a.End,
                    Type = a is GroupMeeting ? "GroupMeeting" : "Normal",
                    ParticipantCount = a is GroupMeeting ? ((GroupMeeting)a).Participants.Count : 0,
                    IsHost = a.CalendarID == calendar.CalendarID,
                    HostName = a.Calendar!.User!.Name,
                    Reminders = a.Reminders.Select(r => new { r.ReminderID, r.Type, r.RemindAt })
                })
                .ToListAsync();

            return (true, appointments, "Success");
        }

        public async Task<(int StatusCode, object? Data, string? ErrorMessage)> AddAppointmentAsync(string userId, CreateAppointmentRequest request)
        {
            var calendar = await _context.Calendars
                .Include(c => c.Appointments)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (calendar == null) return (404, null, "Calendar not found");

            Appointment newApp;
            if (request.Type == "group")
            {
                var user = await _context.Users.FindAsync(userId);
                newApp = new GroupMeeting
                {
                    Name = request.Name,
                    Location = request.Location,
                    Start = request.Start,
                    End = request.End,
                    CalendarID = calendar.CalendarID,
                    Participants = user != null ? new List<User> { user } : new List<User>(),
                    Reminders = request.Reminders?.Select(r => new Reminder
                    {
                        Type = r.Method,
                        RemindAt = r.TriggerTime
                    }).ToList() ?? new List<Reminder>()
                };
            }
            else
            {
                newApp = new Appointment
                {
                    Name = request.Name,
                    Location = request.Location,
                    Start = request.Start,
                    End = request.End,
                    CalendarID = calendar.CalendarID,
                    Reminders = request.Reminders?.Select(r => new Reminder
                    {
                        Type = r.Method,
                        RemindAt = r.TriggerTime
                    }).ToList() ?? new List<Reminder>()
                };
            }

            if (!newApp.CheckValid())
            {
                return (400, null, "Invalid appointment information.");
            }

            if (newApp.Reminders.Any(r => !r.CheckValid(newApp)))
            {
                return (400, null, "Thời gian nhắc nhở phải diễn ra trước giờ bắt đầu của lịch hẹn.");
            }

            if (!request.IgnoreGroupMeeting)
            {
                var groupMeeting = await _context.GroupMeetings
                    .Include(gm => gm.Participants)
                    .FirstOrDefaultAsync(gm => gm.Start < newApp.End && gm.End > newApp.Start);

                if (groupMeeting != null)
                {
                    if (!groupMeeting.Participants.Any(p => p.UserID == userId))
                    {
                        return (409, new { 
                            code = "GROUP_MEETING_MATCH", 
                            message = "Có cuộc họp nhóm đang diễn ra vào thời gian này. Bạn muốn gia nhập không?",
                            meetingId = groupMeeting.AppointmentID 
                        }, null);
                    }
                }
            }

            var conflictingApp = calendar.Appointments.FirstOrDefault(a => a.CheckTrung(newApp));
            if (conflictingApp != null)
            {
                if (!request.ReplaceExisting)
                {
                    return (409, new {
                        code = "TIME_CONFLICT",
                        message = "Time conflict detected.",
                        conflictingId = conflictingApp.AppointmentID
                    }, null);
                }
                else
                {
                    _context.Appointments.Remove(conflictingApp);
                }
            }

            _context.Appointments.Add(newApp);
            await _context.SaveChangesAsync();

            return (200, newApp, null);
        }

        public async Task<(bool IsSuccess, string Message)> JoinGroupMeetingAsync(string meetingId, string userId)
        {
            var groupMeeting = await _context.GroupMeetings
                .Include(g => g.Participants)
                .FirstOrDefaultAsync(g => g.AppointmentID == meetingId);

            var user = await _context.Users.FindAsync(userId);

            if (groupMeeting == null || user == null) return (false, "Group meeting or user not found.");

            groupMeeting.ThemParticipant(user);
            await _context.SaveChangesAsync();

            return (true, "Joined group meeting");
        }
    }
}
