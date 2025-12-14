namespace ElderSharingPrototype.Models
{
    public class MiniRegisterViewModel
    {
        // רמה A
        public string? FirstName { get; set; }
        public int? Age { get; set; }
        public string? Area { get; set; }

        // רמה B+
        public string? Hmo { get; set; }
        public string? Medications { get; set; }

        // רמה C
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }

        // 🔹 חדש – אשראי (רמה C בלבד)
        public string? CreditCardLast4 { get; set; }   // 4 ספרות אחרונות
        public string? CreditCardExpiry { get; set; }  // MM/YY
    }
}
