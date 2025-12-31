using System;

namespace ElderSharingPrototype.Models.Health
{
    public class ReminderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // סוג התזכורת: "Medication" / "Vitals"
        public string Type { get; set; } = "";

        // כותרת שמוצגת למשתמש
        // רמה 1: "תזכורת כללית לנטילת תרופה"
        // רמה 2+: "תזכורת לנטילת: אקמול"
        public string Title { get; set; } = "";

        // שעה בלבד (לשמירה על תאימות לקוד הקיים)
        // לדוגמה: "08:00"
        public string Time { get; set; } = "";

        // ✅ חדש – שם תרופה (רק לרמה 2 ו-3)
        // ברמה 1 נשאר null
        public string? MedicationName { get; set; }

        // הערה חופשית (אופציונלי)
        public string Notes { get; set; } = "";

        // תאריך יצירה
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
