namespace ElderSharingPrototype.Models;

public enum UiAdaptation { Adapted = 1, NotAdapted = 2 }
public enum ExplanationMode { WithExplanation = 1, WithoutExplanation = 2 }
public enum PrivacyLevel { Level1 = 1, Level2 = 2, Level3 = 3 }

public class Participant
{
    public int Id { get; set; }
    public string PseudoId { get; set; } = null!;
    public int GroupNumber { get; set; }
    public UiAdaptation UiAdaptation { get; set; }
    public ExplanationMode ExplanationMode { get; set; }

    public PrivacyLevel? CurrentPrivacyLevel { get; set; }

    // ---- Level B (Privacy Medium) ----
    public string? PersonalIdNumber { get; set; }     // ת"ז אישי (רמה 2 בלבד)
    public string? Hmo { get; set; }                  // קופת חולים
    public string? Phone1 { get; set; }               // טלפון
    public string? Phone2 { get; set; }               // טלפון נוסף (אופציונלי)
    public string? FixedMedications { get; set; }     // רשימת תרופות (מחרוזת)

    // ---- Emergency Contact (B + C) ----
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // ---- Level C (Privacy Low / High Sharing) ----
    public bool MicConsent { get; set; }              // אישור מיקרופון
    public bool CameraConsent { get; set; }           // אישור מצלמה

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ParticipantSession
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAtUtc { get; set; }
}

public class InteractionLog
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = null!;
    public string? Meta { get; set; }
}
