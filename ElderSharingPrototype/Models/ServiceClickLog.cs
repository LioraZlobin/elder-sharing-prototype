using System;
using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models
{
    public class ServiceClickLog
    {
        [Key]
        public int Id { get; set; }

        public int ParticipantId { get; set; }

        [Required]
        public string ServiceKey { get; set; } = "";

        [Required]
        public string ServiceTitle { get; set; } = "";

        public DateTime ClickedAtUtc { get; set; } = DateTime.UtcNow;
    }
}