using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models.Health
{
    public class EmergencyContactTextViewModel
    {
        [Display(Name = "שם איש קשר")]
        [Required(ErrorMessage = "נא להזין שם איש קשר.")]
        public string FullName { get; set; } = "";

        [Display(Name = "טלפון")]
        [Required(ErrorMessage = "נא להזין מספר טלפון.")]
        public string PhoneNumber { get; set; } = "";

        [Display(Name = "מה הקשר אלייך?")]
        public string Relationship { get; set; } = "";

        [Display(Name = "כתובת / מיקום")]
        public string Address { get; set; } = "";

        [Display(Name = "הודעת חירום")]
        public string MessageText { get; set; } = "";

        [Display(Name = "הודעה קבועה (אופציונלי)")]
        public string DefaultMessage { get; set; } = "";
    }
}