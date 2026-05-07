using Microsoft.EntityFrameworkCore;
using CalendarApp.API.Entities;

namespace CalendarApp.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<GroupMeeting> GroupMeetings { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Calendar (1 to 1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Calendar)
                .WithOne(c => c.User)
                .HasForeignKey<Calendar>(c => c.UserID);

            // Calendar - Appointments (1 to Many)
            modelBuilder.Entity<Calendar>()
                .HasMany(c => c.Appointments)
                .WithOne(a => a.Calendar)
                .HasForeignKey(a => a.CalendarID);

            // Appointment - Reminders (1 to Many)
            modelBuilder.Entity<Appointment>()
                .HasMany(a => a.Reminders)
                .WithOne(r => r.Appointment)
                .HasForeignKey(r => r.AppointmentID);

            // GroupMeeting - User (Many to Many Participants)
            modelBuilder.Entity<GroupMeeting>()
                .HasMany(g => g.Participants)
                .WithMany(u => u.GroupMeetings)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupMeetingParticipants",
                    j => j.HasOne<User>().WithMany().HasForeignKey("ParticipantsUserID").OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<GroupMeeting>().WithMany().HasForeignKey("GroupMeetingsAppointmentID").OnDelete(DeleteBehavior.Cascade)
                );

            // Table-Per-Hierarchy for Appointment / GroupMeeting
            modelBuilder.Entity<Appointment>()
                .HasDiscriminator<string>("AppointmentType")
                .HasValue<Appointment>("Normal")
                .HasValue<GroupMeeting>("GroupMeeting");
        }
    }
}
