using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models.Health
{
    public class EmergencyVideoViewModel
    {
        [Required(ErrorMessage = "נא להזין שם מלא")]
        [Display(Name = "שם מלא")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "נא להזין מספר טלפון")]
        [Display(Name = "טלפון")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "קישור לסרטון (אופציונלי)")]
        public string? VideoUrl { get; set; }

        [Display(Name = "הודעה קצרה (אופציונלי)")]
        public string? Notes { get; set; }
    }
}
