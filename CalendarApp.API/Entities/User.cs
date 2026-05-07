using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CalendarApp.API.Entities
{
    public class User
    {
        public string UserID { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        
        public string Username { get; set; } = string.Empty;
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonIgnore]
        public Calendar? Calendar { get; set; }
        [JsonIgnore]
        public List<GroupMeeting> GroupMeetings { get; set; } = new();
    }
}
