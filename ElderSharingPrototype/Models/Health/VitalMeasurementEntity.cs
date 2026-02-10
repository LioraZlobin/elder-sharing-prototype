using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElderSharingPrototype.Models.Health
{
    public class VitalMeasurementEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ParticipantId { get; set; }

        // "BloodPressure" / "Sugar"
        [Required]
        public string Kind { get; set; } = "";

        public int? Systolic { get; set; }
        public int? Diastolic { get; set; }

        public int? SugarMgDl { get; set; }

        public string? Notes { get; set; }

        public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
    }
}
