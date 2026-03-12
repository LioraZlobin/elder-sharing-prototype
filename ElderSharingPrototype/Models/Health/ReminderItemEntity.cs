using System;
using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models.Health
{
    public class ReminderItemEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ParticipantId { get; set; }

        // "Vitals" / "Medication"
        [Required]
        public string Type { get; set; } = "";

        public string Title { get; set; } = "";
        public string Time { get; set; } = "";

        public string? MedicationName { get; set; }
        public string? MedicationMode { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
