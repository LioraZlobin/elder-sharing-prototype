using System;
using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models.Health
{
    public class EmergencyVideoEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ParticipantId { get; set; }

        public string? VideoUrl { get; set; }
        public string? Notes { get; set; }

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
