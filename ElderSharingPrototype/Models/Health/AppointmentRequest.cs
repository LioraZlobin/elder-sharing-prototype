using System;

namespace ElderSharingPrototype.Models.Health
{
    public class AppointmentRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string TreatmentArea { get; set; } = ""; // ✅ חדש – תחום טיפול
        public string Reason { get; set; } = "";

        public string PreferredDate { get; set; } = ""; // "2025-01-01"
        public string PreferredTime { get; set; } = ""; // "10:30"

        public string Notes { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
