using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CalendarApp.API.Entities;

namespace CalendarApp.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // context.Database.EnsureDeleted(); // Removed to prevent dropping database on startup
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            // Create users with proper hashed passwords
            var user1 = new User { UserID = "user1", Name = "Việt", Username = "viet", PasswordHash = HashPassword("123456") };
            var user2 = new User { UserID = "user2", Name = "Trần Út", Username = "tranut", PasswordHash = HashPassword("123456") };

            var cal1 = new Calendar { CalendarID = "cal1", UserID = "user1" };
            var cal2 = new Calendar { CalendarID = "cal2", UserID = "user2" };

            // Group Meeting (user 1 is in it)
            var groupMeeting = new GroupMeeting
            {
                AppointmentID = "gm1",
                Name = "Họp Dự Án",
                Location = "Phòng họp A",
                Start = DateTime.Today.AddHours(14), // 2:00 PM today
                End = DateTime.Today.AddHours(15),   // 3:00 PM today
                CalendarID = "cal1",
                Participants = new List<User> { user1 }
            };

            // Normal appointment for User 1 with a Reminder
            var app1 = new Appointment
            {
                AppointmentID = "app1",
                Name = "Khám răng",
                Location = "Phòng khám",
                Start = DateTime.Today.AddHours(9),
                End = DateTime.Today.AddHours(10),
                CalendarID = "cal1"
            };

            var rem1 = new Reminder
            {
                ReminderID = "rem1",
                AppointmentID = "app1",
                RemindAt = DateTime.Today.AddHours(8),
                Type = "EMAIL"
            };

            app1.Reminders.Add(rem1);

            // Normal appointment for User 2 with a Reminder
            var app2 = new Appointment
            {
                AppointmentID = "app2",
                Name = "Gặp khách hàng",
                Location = "Quán Cafe",
                Start = DateTime.Today.AddHours(10),
                End = DateTime.Today.AddHours(11),
                CalendarID = "cal2"
            };

            var rem2 = new Reminder
            {
                ReminderID = "rem2",
                AppointmentID = "app2",
                RemindAt = DateTime.Today.AddHours(9).AddMinutes(45),
                Type = "EMAIL"
            };

            app2.Reminders.Add(rem2);

            context.Users.AddRange(user1, user2);
            context.Calendars.AddRange(cal1, cal2);
            context.Appointments.AddRange(app1, app2, groupMeeting);
            
            context.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
