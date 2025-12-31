namespace ElderSharingPrototype.Models
{
    public class MiniRegisterViewModel
    {
        // רמה 1 – בסיסי
        public string? FirstName { get; set; }
        public string? Language { get; set; }
        public int? Age { get; set; }


        // רמה 2 – בינוני
        public List<string> SelectedMedications { get; set; } = new();

        public string? FixedMedications { get; set; } // תרופות קבועות
        public List<string> SelectedDiseases { get; set; } = new();
        public string? FixedDiseases { get; set; } // מחלות

        // רמה 3 – גבוה
        public string? IdNumber { get; set; } // ת"ז לזיהוי מול קופת חולים (דמו)
    }
}
