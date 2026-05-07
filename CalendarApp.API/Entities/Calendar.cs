using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CalendarApp.API.Entities
{
    public class Calendar
    {
        public string CalendarID { get; set; } = Guid.NewGuid().ToString();
        public string UserID { get; set; } = string.Empty;
        
        [JsonIgnore]
        public User? User { get; set; }
        
        public List<Appointment> Appointments { get; set; } = new();
    }
}
