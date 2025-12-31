using System;

namespace ElderSharingPrototype.Models.Health
{
    public class VitalMeasurement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Kind { get; set; } = ""; // "BloodPressure" / "Sugar"
        public int? Systolic { get; set; }      // ל"ד עליון
        public int? Diastolic { get; set; }     // ל"ד תחתון
        public int? SugarMgDl { get; set; }     // סוכר
        public DateTime MeasuredAt { get; set; } = DateTime.Now;
        public string Notes { get; set; } = "";
    }
}
