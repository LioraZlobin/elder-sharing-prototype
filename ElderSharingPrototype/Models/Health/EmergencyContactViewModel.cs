using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models.Health
{
    public class EmergencyContactViewModel
    {
        [Required(ErrorMessage = "נא להזין שם מלא")]
        [Display(Name = "שם מלא")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "נא להזין מספר טלפון")]
        [Display(Name = "טלפון")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "קרבה")]
        public string? Relationship { get; set; }

        // ✅ זה פותר את השגיאה Model.Message
        [Display(Name = "הודעה/הערה")]
        public string? Message { get; set; }
    }
}
