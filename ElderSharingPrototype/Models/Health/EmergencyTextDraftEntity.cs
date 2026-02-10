using System;
using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models.Health
{
    public class EmergencyTextDraftEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ParticipantId { get; set; }

        public string? Address { get; set; }
        public string? MessageText { get; set; }

        public bool LastSendSucceededDemo { get; set; } = false;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
