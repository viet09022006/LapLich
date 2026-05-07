using System;
using System.Collections.Generic;

namespace CalendarApp.API.Entities
{
    public class GroupMeeting : Appointment
    {
        public List<User> Participants { get; set; } = new();

        public void ThemParticipant(User user)
        {
            if (!Participants.Contains(user))
            {
                Participants.Add(user);
            }
        }

        public bool CheckNameAndDuration(string name, TimeSpan duration)
        {
            return this.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && this.GetDuration() == duration;
        }
    }
}
