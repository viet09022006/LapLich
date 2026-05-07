namespace CalendarApp.API.DTOs
{
    public class ReminderDto
    {
        public string Method { get; set; } = string.Empty;
        public DateTime TriggerTime { get; set; }
    }

    public class CreateAppointmentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Type { get; set; } = "normal";
        public bool ReplaceExisting { get; set; } = false;
        public bool IgnoreGroupMeeting { get; set; } = false;
        public List<ReminderDto>? Reminders { get; set; }
    }
}
