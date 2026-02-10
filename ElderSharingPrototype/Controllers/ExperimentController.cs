using ElderSharingPrototype.Data;
using ElderSharingPrototype.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElderSharingPrototype.Controllers;

public class ExperimentController : Controller
{
    private readonly AppDbContext _db;

    public ExperimentController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Experiment/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    // POST: /Experiment/Login
    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var serial = (model.SerialNumber ?? "").Trim();

        int groupNumber = GetGroupFromSerial(serial);
        if (groupNumber == 0)
        {
            ModelState.AddModelError("", "מספר סידורי לא תקין. יש להזין בפורמט כמו 1.1 / 2.2 / 3.1 / 4.7");
            return View(model);
        }

        var (uiAdaptation, explanationMode) = GetExperimentCondition(groupNumber);

        var participant = _db.Participants.FirstOrDefault(p => p.PseudoId == serial);

        if (participant == null)
        {
            participant = new Participant
            {
                PseudoId = serial,
                GroupNumber = groupNumber,
                UiAdaptation = uiAdaptation,
                ExplanationMode = explanationMode
            };

            _db.Participants.Add(participant);
            _db.SaveChanges();
        }
        else
        {
            // עקביות ניסויית
            groupNumber = participant.GroupNumber;
            uiAdaptation = participant.UiAdaptation;
            explanationMode = participant.ExplanationMode;
        }

        // פתיחת סשן ניסוי (כל כניסה יוצרת סשן חדש)
        var session = new ParticipantSession
        {
            ParticipantId = participant.Id
        };
        _db.ParticipantSessions.Add(session);

        _db.InteractionLogs.Add(new InteractionLog
        {
            ParticipantId = participant.Id,
            Action = "Login",
            Meta = $"Serial={serial}, Group={groupNumber}, Ui={uiAdaptation}, Explanation={explanationMode}"
        });

        _db.SaveChanges();

        // Session בסיסי
        HttpContext.Session.SetInt32("ParticipantId", participant.Id);
        HttpContext.Session.SetInt32("GroupNumber", groupNumber);
        HttpContext.Session.SetInt32("UiAdaptation", (int)uiAdaptation);
        HttpContext.Session.SetInt32("ExplanationMode", (int)explanationMode);
        HttpContext.Session.SetString("IsLoggedIn", "true");

        // ✅ טעינת "פרופיל" חדש מה-DB ל-Session
        SetOrRemove("PersonalIdNumber", participant.PersonalIdNumber);
        SetOrRemove("Hmo", participant.Hmo);
        SetOrRemove("Phone1", participant.Phone1);
        SetOrRemove("Phone2", participant.Phone2);
        SetOrRemove("FixedMedications", participant.FixedMedications);
        SetOrRemove("EmergencyContactName", participant.EmergencyContactName);
        SetOrRemove("EmergencyContactPhone", participant.EmergencyContactPhone);

        HttpContext.Session.SetString("MicConsent", participant.MicConsent ? "true" : "false");
        HttpContext.Session.SetString("CameraConsent", participant.CameraConsent ? "true" : "false");

        // ✅ אם כבר נבחרה רמת שיתוף בעבר – נכנסים ישר לשירותים
        if (participant.CurrentPrivacyLevel.HasValue)
        {
            var sharing = participant.CurrentPrivacyLevel.Value switch
            {
                PrivacyLevel.Level1 => "A",
                PrivacyLevel.Level2 => "B",
                PrivacyLevel.Level3 => "C",
                _ => "A"
            };

            HttpContext.Session.SetString("SharingLevel", sharing);
            HttpContext.Session.SetString("IsLoggedIn", "true");

            _db.InteractionLogs.Add(new InteractionLog
            {
                ParticipantId = participant.Id,
                Action = "LoginReturningUser",
                Meta = $"Serial={serial}, SharingLevel={sharing}"
            });
            _db.SaveChanges();

            return RedirectToAction("Services", "Prototype");
        }

        return RedirectToAction("Intro", "Prototype");
    }

    private void SetOrRemove(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            HttpContext.Session.SetString(key, value);
        else
            HttpContext.Session.Remove(key);
    }

    private int GetGroupFromSerial(string serial)
    {
        var parts = serial.Split('.');
        if (parts.Length != 2) return 0;
        if (!int.TryParse(parts[0], out int group)) return 0;
        if (group < 1 || group > 4) return 0;
        return group;
    }

    private (UiAdaptation ui, ExplanationMode expl) GetExperimentCondition(int groupNumber)
    {
        return groupNumber switch
        {
            1 => (UiAdaptation.Adapted, ExplanationMode.WithExplanation),
            2 => (UiAdaptation.Adapted, ExplanationMode.WithoutExplanation),
            3 => (UiAdaptation.NotAdapted, ExplanationMode.WithExplanation),
            4 => (UiAdaptation.NotAdapted, ExplanationMode.WithoutExplanation),
            _ => (UiAdaptation.NotAdapted, ExplanationMode.WithExplanation)
        };
    }
}
