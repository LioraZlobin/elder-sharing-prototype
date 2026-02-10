using ElderSharingPrototype.Models;
using ElderSharingPrototype.Models.Health;
using Microsoft.EntityFrameworkCore;

namespace ElderSharingPrototype.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<ParticipantSession> ParticipantSessions => Set<ParticipantSession>();
    public DbSet<InteractionLog> InteractionLogs => Set<InteractionLog>();
    public DbSet<VitalMeasurementEntity> VitalMeasurements => Set<VitalMeasurementEntity>();
    public DbSet<ReminderItemEntity> ReminderItems => Set<ReminderItemEntity>();
    public DbSet<AppointmentRequestEntity> AppointmentRequests => Set<AppointmentRequestEntity>();
    public DbSet<EmergencyContactEntity> EmergencyContacts => Set<EmergencyContactEntity>();
    public DbSet<EmergencyTextDraftEntity> EmergencyTextDrafts => Set<EmergencyTextDraftEntity>();
    public DbSet<EmergencyVideoEntity> EmergencyVideos => Set<EmergencyVideoEntity>();
}
