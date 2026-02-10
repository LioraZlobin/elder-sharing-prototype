using System.ComponentModel.DataAnnotations;

namespace ElderSharingPrototype.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "יש להזין מספר סידורי")]
    [Display(Name = "מספר סידורי")]
    public string SerialNumber { get; set; } = string.Empty;
}
