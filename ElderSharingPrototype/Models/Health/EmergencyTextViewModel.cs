using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models.Health
{
    public class EmergencyTextViewModel
    {
        [Required(ErrorMessage = "נא להזין שם מלא")]
        [Display(Name = "שם מלא")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "נא להזין מספר טלפון")]
        [Display(Name = "טלפון")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "כתובת (אופציונלי)")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "נא להזין טקסט הודעה")]
        [Display(Name = "טקסט ההודעה")]
        public string MessageText { get; set; } = string.Empty;
    }
}
