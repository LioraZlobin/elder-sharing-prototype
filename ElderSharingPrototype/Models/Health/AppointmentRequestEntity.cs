using System;
using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models.Health
{
    public class AppointmentRequestEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ParticipantId { get; set; }

        public string TreatmentArea { get; set; } = "";
        public string Reason { get; set; } = "";
        public string PreferredDate { get; set; } = "";
        public string PreferredTime { get; set; } = "";
        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
