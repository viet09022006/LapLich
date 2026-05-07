using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CalendarApp.API.Entities
{
    public class Appointment
    {
        public string AppointmentID { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string CalendarID { get; set; } = string.Empty;
        
        [JsonIgnore]
        public Calendar? Calendar { get; set; }
        
        public List<Reminder> Reminders { get; set; } = new();

        public bool CheckValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && Start < End;
        }

        public TimeSpan GetDuration()
        {
            return End - Start;
        }

        public bool CheckTrung(Appointment other)
        {
            return Start < other.End && other.Start < End;
        }
    }
}
