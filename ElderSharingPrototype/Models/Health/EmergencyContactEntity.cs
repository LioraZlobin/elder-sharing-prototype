using System;
using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models.Health
{
    public class EmergencyContactEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ParticipantId { get; set; }

        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        public string? Relationship { get; set; }
        public string? DefaultMessage { get; set; }

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
