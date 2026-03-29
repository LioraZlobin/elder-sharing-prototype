namespace ElderSharingPrototype.Models
{
    public class MiniRegisterViewModel
    {
        // Level 2 (B) + Level 3 (C)
        public string? PersonalIdNumber { get; set; }   // ת"ז אישי
        public string? Hmo { get; set; }                // קופת חולים
        public string? Phone1 { get; set; }             // טלפון
        public string? Phone2 { get; set; }             // טלפון נוסף (אופציונלי)

        // תרופות
        // חשוב: nullable כדי למנוע הודעת required אוטומטית באנגלית
        public List<string>? SelectedMedications { get; set; }
        public string? FixedMedications { get; set; }

        // איש קשר לחירום (B + C)
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }

        // Level 3 (C)
        public bool MicConsent { get; set; }
        public bool CameraConsent { get; set; }
    }
}