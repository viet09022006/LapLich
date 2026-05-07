using System;
using System.Text.Json.Serialization;

namespace CalendarApp.API.Entities
{
    public class Reminder
    {
        public string ReminderID { get; set; } = Guid.NewGuid().ToString();
        public DateTime RemindAt { get; set; }
        public string Type { get; set; } = "Popup";

        public string AppointmentID { get; set; } = string.Empty;
        [JsonIgnore]
        public Appointment? Appointment { get; set; }

        public bool CheckValid(Appointment app)
        {
            return RemindAt <= app.Start;
        }
    }
}
