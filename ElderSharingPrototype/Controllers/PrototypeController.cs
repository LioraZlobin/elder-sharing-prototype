using ElderSharingPrototype.Data;
using ElderSharingPrototype.Models;
using Microsoft.AspNetCore.Mvc;
using ElderSharingPrototype.Helpers;

namespace ElderSharingPrototype.Controllers
{
    public class PrototypeController : Controller
    {
        private readonly AppDbContext _db;

        public PrototypeController(AppDbContext db)
        {
            _db = db;
        }

        // -----------------------------
        // Helpers
        // -----------------------------
        private int? GetParticipantId() => HttpContext.Session.GetInt32("ParticipantId");

        private Participant? GetCurrentParticipant(int participantId)
            => _db.Participants.FirstOrDefault(p => p.Id == participantId);

        private void SetUiFlagsToViewBag()
        {
            ViewBag.GroupNumber = HttpContext.Session.GetInt32("GroupNumber") ?? 0;
            ViewBag.UiAdaptation = HttpContext.Session.GetInt32("UiAdaptation") ?? (int)UiAdaptation.NotAdapted;
            ViewBag.ExplanationMode = HttpContext.Session.GetInt32("ExplanationMode") ?? (int)ExplanationMode.WithExplanation;
        }

        private void LogAction(int participantId, string action, string? meta = null)
        {
            _db.InteractionLogs.Add(new InteractionLog
            {
                ParticipantId = participantId,
                Action = action,
                Meta = meta
            });
            _db.SaveChanges();
        }

        private PrivacyLevel MapSharingLevelToPrivacyLevel(string sharingLevel)
        {
            return sharingLevel switch
            {
                "A" => PrivacyLevel.Level1,
                "B" => PrivacyLevel.Level2,
                "C" => PrivacyLevel.Level3,
                _ => PrivacyLevel.Level1
            };
        }

        private string MapPrivacyLevelToSharingLevel(PrivacyLevel level)
        {
            return level switch
            {
                PrivacyLevel.Level1 => "A",
                PrivacyLevel.Level2 => "B",
                PrivacyLevel.Level3 => "C",
                _ => "A"
            };
        }

        private void SyncSharingLevelFromDbToSessionIfExists(int participantId)
        {
            var p = GetCurrentParticipant(participantId);
            if (p?.CurrentPrivacyLevel != null)
                HttpContext.Session.SetString("SharingLevel", MapPrivacyLevelToSharingLevel(p.CurrentPrivacyLevel.Value));
        }

        private static List<string> GetMedicationCatalog()
        {
            return new List<string>
            {
                "אקמול","אדויל","אספירין","אומפרדקס","אומפרזול","אנטיהיסטמין",
                "ברופן","בטאקורטן","ביסופרולול","ברזל","בודיקורט",
                "גלוקופאג׳","גסטרו","ג׳לוסיל","גאבאפנטין","גינרה",
                "דקסמול","דקסמול קולד","דופסטון","דיאמיקרון","דוקסילין",
                "הידרוכלורותיאזיד","הידרוקסיזין","הפארין",
                "ויטמין D","ויטמין B12","וולטרן","ורמוקס",
                "זודורם","זירקט","זינאט",
                "חומצה פולית",
                "טאמסולין","טמסולוסין","טראמדקס","טוביאז",
                "יאז","יוויטל",
                "כדורי ברזל","כדורי סידן",
                "לוסק","לוריוון","לורטאדין","לבותרוקסין","לנטוס",
                "מטפורמין","מוקסיפן","מגנזיום","מיקרופירין","מונולונג",
                "נורופן","נקסיום","נורמלקס","נורמיטן",
                "סטטינים","סימוביל","סימבסטטין","סולפיריד","סולפיד",
                "פראמין","פרדניזון","פרדניזולון","פלביקס","פנרגן",
                "ציפרודקס","ציפרופלוקסצין","ציפרודין","ציפרלקס","צנטרום",
                "קונקור","קלונקס","קלקסן","קווינפריל","קולכיצין",
                "ריטלין","רוקסט","רניטידין","רמרון",
                "תירוקסין","תמיפלו"
            };
        }

        // -----------------------------
        // Intro
        // -----------------------------
        public IActionResult Intro()
        {
            var pid = GetParticipantId();
            if (pid == null)
                return RedirectToAction("Login", "Experiment");

            SetUiFlagsToViewBag();
            LogAction(pid.Value, "View:Intro");
            return View();
        }

        // -----------------------------
        // Choose
        // -----------------------------
        [HttpGet]
        public IActionResult Choose()
        {
            var pid = GetParticipantId();
            if (pid == null)
                return RedirectToAction("Login", "Experiment");

            SetUiFlagsToViewBag();
            SyncSharingLevelFromDbToSessionIfExists(pid.Value);

            LogAction(pid.Value, "View:Choose");
            return View();
        }

        [HttpPost]
        public IActionResult Choose(string level)
        {
            var pid = GetParticipantId();
            if (pid == null)
                return RedirectToAction("Login", "Experiment");

            var chosen = (level ?? "A").Trim().ToUpperInvariant();
            if (chosen != "A" && chosen != "B" && chosen != "C")
                chosen = "A";

            HttpContext.Session.SetString("SharingLevel", chosen);

            var participant = GetCurrentParticipant(pid.Value);
            if (participant != null)
            {
                participant.CurrentPrivacyLevel = MapSharingLevelToPrivacyLevel(chosen);
                _db.SaveChanges();
            }

            LogAction(pid.Value, "SelectPrivacyLevel", $"Level={chosen}");
            return RedirectToAction("RegisterMini");
        }

        // -----------------------------
        // RegisterMini (GET)
        // -----------------------------
        [HttpGet]
        public IActionResult RegisterMini()
        {
            var pid = GetParticipantId();
            if (pid == null)
                return RedirectToAction("Login", "Experiment");

            SyncSharingLevelFromDbToSessionIfExists(pid.Value);

            var p = GetCurrentParticipant(pid.Value);
            if (p != null)
            {
                // Session sync if missing
                SetIfMissing("PersonalIdNumber", p.PersonalIdNumber);
                SetIfMissing("Hmo", p.Hmo);
                SetIfMissing("Phone1", p.Phone1);
                SetIfMissing("Phone2", p.Phone2);
                SetIfMissing("FixedMedications", p.FixedMedications);
                SetIfMissing("EmergencyContactName", p.EmergencyContactName);
                SetIfMissing("EmergencyContactPhone", p.EmergencyContactPhone);

                HttpContext.Session.SetString("MicConsent", p.MicConsent ? "true" : "false");
                HttpContext.Session.SetString("CameraConsent", p.CameraConsent ? "true" : "false");

                if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("SharingLevel")) && p.CurrentPrivacyLevel.HasValue)
                    HttpContext.Session.SetString("SharingLevel", MapPrivacyLevelToSharingLevel(p.CurrentPrivacyLevel.Value));
            }

            SetUiFlagsToViewBag();

            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";
            ViewBag.Level = level;
            ViewBag.MedCatalog = GetMedicationCatalog();
            ViewBag.HmoList = new List<string> { "כללית", "מכבי", "מאוחדת", "לאומית" };

            var savedMeds = HttpContext.Session.GetString("FixedMedications") ?? "";
            var selectedMeds = savedMeds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var vm = new MiniRegisterViewModel
            {
                PersonalIdNumber = HttpContext.Session.GetString("PersonalIdNumber"),
                Hmo = HttpContext.Session.GetString("Hmo"),
                Phone1 = HttpContext.Session.GetString("Phone1"),
                Phone2 = HttpContext.Session.GetString("Phone2"),

                FixedMedications = savedMeds,
                SelectedMedications = selectedMeds,

                // נשארים ב-VM כדי לא לשבור קומפילציה, אבל לא מחייבים אותם במסך הזה
                EmergencyContactName = HttpContext.Session.GetString("EmergencyContactName"),
                EmergencyContactPhone = HttpContext.Session.GetString("EmergencyContactPhone"),

                MicConsent = (HttpContext.Session.GetString("MicConsent") ?? "false") == "true",
                CameraConsent = (HttpContext.Session.GetString("CameraConsent") ?? "false") == "true"
            };

            LogAction(pid.Value, "View:RegisterMini", $"Level={level}");
            return View(vm);
        }

        private void SetIfMissing(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(key)) && !string.IsNullOrWhiteSpace(value))
                HttpContext.Session.SetString(key, value);
        }

        // -----------------------------
        // RegisterMini (POST)
        // -----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterMini(MiniRegisterViewModel model)
        {
            var pid = GetParticipantId();
            if (pid == null)
                return RedirectToAction("Login", "Experiment");

            SetUiFlagsToViewBag();

            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";
            ViewBag.Level = level;
            ViewBag.MedCatalog = GetMedicationCatalog();
            ViewBag.HmoList = new List<string> { "כללית", "מכבי", "מאוחדת", "לאומית" };

            // -------- Validations --------
            // A: אין מה למלא
            if (level == "B" || level == "C")
            {
                var pidNum = (model.PersonalIdNumber ?? "").Trim();
                if (pidNum.Length != 9 || !pidNum.All(char.IsDigit))
                    ModelState.AddModelError(nameof(model.PersonalIdNumber), "נא להזין תעודת זהות תקינה (9 ספרות).");

                if (string.IsNullOrWhiteSpace(model.Hmo))
                    ModelState.AddModelError(nameof(model.Hmo), "נא לבחור קופת חולים.");

                if (string.IsNullOrWhiteSpace(model.Phone1))
                    ModelState.AddModelError(nameof(model.Phone1), "נא להזין מספר טלפון.");

                // אם את רוצה לחייב תרופות ברמה B/C השאירי את זה.
                // אם לא – תמחקי את הבלוק הבא.
                if (model.SelectedMedications == null || !model.SelectedMedications.Any())
                    ModelState.AddModelError(nameof(model.SelectedMedications), "נא לבחור לפחות תרופה אחת מהרשימה.");

                // ❗ חשוב: לא מחייבים כאן איש קשר לחירום כי המסך הזה לא כולל שדות לזה
                // Emergency Contact נשמר/מוזן במסך /Health/EmergencyContact
            }

            if (level == "C")
            {
                if (!model.MicConsent)
                    ModelState.AddModelError(nameof(model.MicConsent), "כדי להשתמש בשירותי רמה 3 יש לאשר הפעלת מיקרופון.");
                if (!model.CameraConsent)
                    ModelState.AddModelError(nameof(model.CameraConsent), "כדי להשתמש בשירותי רמה 3 יש לאשר הפעלת מצלמה.");
            }

            if (!ModelState.IsValid)
                return View(model);

            // -------- Save to Session --------
            if (level == "A")
            {
                // אין פרטים לשמור
                HttpContext.Session.SetString("MicConsent", "false");
                HttpContext.Session.SetString("CameraConsent", "false");
            }
            else
            {
                HttpContext.Session.SetString("PersonalIdNumber", (model.PersonalIdNumber ?? "").Trim());
                HttpContext.Session.SetString("Hmo", (model.Hmo ?? "").Trim());
                HttpContext.Session.SetString("Phone1", (model.Phone1 ?? "").Trim());

                if (!string.IsNullOrWhiteSpace(model.Phone2))
                    HttpContext.Session.SetString("Phone2", model.Phone2.Trim());
                else
                    HttpContext.Session.Remove("Phone2");

                HttpContext.Session.SetString("FixedMedications", string.Join(", ", model.SelectedMedications ?? new List<string>()));

                if (level == "C")
                {
                    HttpContext.Session.SetString("MicConsent", model.MicConsent ? "true" : "false");
                    HttpContext.Session.SetString("CameraConsent", model.CameraConsent ? "true" : "false");
                }
                else
                {
                    HttpContext.Session.SetString("MicConsent", "false");
                    HttpContext.Session.SetString("CameraConsent", "false");
                }
            }

            HttpContext.Session.SetString("IsLoggedIn", "true");
            LogAction(pid.Value, "Submit:RegisterMini", $"Level={level}");

            // -------- Save to DB --------
            var participant = GetCurrentParticipant(pid.Value);
            if (participant != null)
            {
                participant.CurrentPrivacyLevel = MapSharingLevelToPrivacyLevel(level);

                if (level == "A")
                {
                    participant.PersonalIdNumber = null;
                    participant.Hmo = null;
                    participant.Phone1 = null;
                    participant.Phone2 = null;
                    participant.FixedMedications = null;

                    // לא מוחקים EmergencyContact כאן, כי זה שירות שממולא בנפרד
                    participant.MicConsent = false;
                    participant.CameraConsent = false;
                }
                else
                {
                    participant.PersonalIdNumber = (model.PersonalIdNumber ?? "").Trim();
                    participant.Hmo = (model.Hmo ?? "").Trim();
                    participant.Phone1 = (model.Phone1 ?? "").Trim();
                    participant.Phone2 = string.IsNullOrWhiteSpace(model.Phone2) ? null : model.Phone2.Trim();
                    participant.FixedMedications = string.Join(", ", model.SelectedMedications ?? new List<string>());

                    if (level == "C")
                    {
                        participant.MicConsent = model.MicConsent;
                        participant.CameraConsent = model.CameraConsent;
                    }
                    else
                    {
                        participant.MicConsent = false;
                        participant.CameraConsent = false;
                    }
                }

                _db.SaveChanges();
            }

            // ✅ מעבר חד משמעי לשירותים
            return RedirectToAction("Services", "Prototype");
        }


        // -----------------------------
        // Services
        // -----------------------------
        public IActionResult Services()
        {
            SetUiFlagsToViewBag();

            var pid = GetParticipantId();
            if (pid == null)
                return RedirectToAction("Login", "Experiment");

            SyncSharingLevelFromDbToSessionIfExists(pid.Value);

            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";
            ViewBag.Level = level;

            // 🤖 כל השירותים האפשריים עם רמות
            var robotOptions = new List<(string Key, string LevelCode, string Text, string Url, string AudioFile, string ButtonText)>
    {
        // רמה 1
        ("alert_warning","A","כאן מוצגת התרעה קולית במקרה של מצב חריג כמו חום גבוה או סכנה סביבתית.","/Health/AudioAlerts","alert_warning.mp3","פתחי התרעה קולית"),
        ("doctor_recommendation","A","כאן ניתן לקבל המלצה כללית לביקור אצל רופא או לבצע בדיקות שגרתיות.","/Health/CheckupRecommendations","doctor_recommendation.mp3","פתחי המלצה לרופא"),
        ("measurements_reminder","A","כאן ניתן לקבל תזכורת לבדיקת מדדים רפואיים כמו לחץ דם או סוכר.","/Health/VitalsReminders","measurements_reminder.mp3","פתחי תזכורת למדדים"),
        ("medication_basic","A","כאן ניתן ליצור תזכורת כללית ללקיחת תרופות.","/Health/MedicationReminders?mode=basic","medication_basic.mp3","פתחי תזכורת לתרופות"),

        // רמה 2
        ("emergency_contact","B","כאן ניתן לשמור איש קשר לחירום ולשלוח אליו הודעה במקרה הצורך.","/Health/EmergencyContact","emergency_contact.mp3","פתחי איש קשר לחירום"),
        ("doctor_appointment","B","כאן ניתן להזמין תור לרופא לפי תחום הטיפול.","/Health/AppointmentRequest","doctor_appointment.mp3","פתחי זימון תור"),
        ("dynamic_measurements","B","כאן ניתן לבצע מדידה חד פעמית של מדדים רפואיים. המדידה מוצגת על המסך בלבד ואינה נשמרת בהיסטוריה.","/Health/Measurements","dynamic_measurements.mp3","פתחי מדדים רפואיים"),
        ("medication_management","B","כאן ניתן לנהל רשימת תרופות קבועות ולהוסיף תזכורות.","/Health/MedicationReminders?mode=detailed","medication_management.mp3","פתחי ניהול תרופות"),

        // רמה 3
        ("video_contact","C","כאן ניתן ליצור שיחת וידאו עם איש קשר קרוב במקרה חירום.","/Health/EmergencyVideo","video_contact.mp3","פתחי שיחת וידאו"),
        ("doctor_video_call","C","כאן ניתן לבצע שיחת טלפון או וידאו עם רופא.","/Health/TeleVisit","doctor_video_call.mp3","פתחי תור וידאו לרופא"),
        ("measurements_history","C","כאן ניתן לראות היסטוריית מדדים רפואיים ולהשוות בין מדידות.","/Health/Measurements","measurements_history.mp3","פתחי היסטוריית מדדים"),
        ("pharmacy_contact","C","כאן ניתן לבדוק מלאי תרופות וליצור קשר עם בית מרקחת.","/Health/PharmacyContact","pharmacy_contact.mp3","פתחי קשר עם בית מרקחת")
    };

            // 🔎 פילטור לפי רמה יורשת
            var availableRobotOptions = robotOptions
                .Where(x =>
                    level == "A" ? x.LevelCode == "A" :
                    level == "B" ? (x.LevelCode == "A" || x.LevelCode == "B") :
                                   (x.LevelCode == "A" || x.LevelCode == "B" || x.LevelCode == "C"))
                .ToList();

            // 🚫 לא לחזור פעמיים ברצף
            var lastRobotKey = HttpContext.Session.GetString("LastRobotServiceKey");
            var optionsWithoutLast = availableRobotOptions.Where(x => x.Key != lastRobotKey).ToList();
            if (optionsWithoutLast.Any())
                availableRobotOptions = optionsWithoutLast;

            var rnd = new Random();
            var selectedRobot = availableRobotOptions[rnd.Next(availableRobotOptions.Count)];

            HttpContext.Session.SetString("LastRobotServiceKey", selectedRobot.Key);

            // מה שיוצג בדף
            ViewBag.RobotRecKey = selectedRobot.Key;
            ViewBag.RobotRecText = selectedRobot.Text;
            ViewBag.RobotRecUrl = selectedRobot.Url;
            ViewBag.RobotRecAudio = selectedRobot.AudioFile;
            ViewBag.RobotRecButtonText = selectedRobot.ButtonText;
            ViewBag.RobotRecIsQuestion = false;

            LogAction(pid.Value, "View:Services", $"Level={level}, RecKey={selectedRobot.Key}");

            return View();
        }


        // Upgrade (A->B, B->C)
        [HttpPost]
        public IActionResult UpgradeLevel()
        {
            var pid = GetParticipantId();
            if (pid == null)
                return RedirectToAction("Login", "Experiment");

            var current = HttpContext.Session.GetString("SharingLevel") ?? "A";
            var next = current == "A" ? "B" : current == "B" ? "C" : "C";

            HttpContext.Session.SetString("SharingLevel", next);

            var participant = GetCurrentParticipant(pid.Value);
            if (participant != null)
            {
                participant.CurrentPrivacyLevel = MapSharingLevelToPrivacyLevel(next);
                _db.SaveChanges();
            }

            LogAction(pid.Value, "UpgradePrivacyLevel", $"From={current}, To={next}");
            return RedirectToAction("RegisterMini");
        }

        // Downgrade + clear irrelevant data
        [HttpPost]
        public IActionResult DowngradeLevel(string targetLevel)
        {
            var pid = GetParticipantId();
            if (pid == null)
                return RedirectToAction("Login", "Experiment");

            var t = (targetLevel ?? "A").Trim().ToUpperInvariant();
            if (t != "A" && t != "B" && t != "C") t = "A";

            HttpContext.Session.SetString("SharingLevel", t);

            if (t == "A")
            {
                HttpContext.Session.Remove("PersonalIdNumber");
                HttpContext.Session.Remove("Hmo");
                HttpContext.Session.Remove("Phone1");
                HttpContext.Session.Remove("Phone2");
                HttpContext.Session.Remove("FixedMedications");
                HttpContext.Session.Remove("EmergencyContactName");
                HttpContext.Session.Remove("EmergencyContactPhone");
                HttpContext.Session.SetString("MicConsent", "false");
                HttpContext.Session.SetString("CameraConsent", "false");
            }
            else if (t == "B")
            {
                HttpContext.Session.SetString("MicConsent", "false");
                HttpContext.Session.SetString("CameraConsent", "false");
            }

            var participant = GetCurrentParticipant(pid.Value);
            if (participant != null)
            {
                participant.CurrentPrivacyLevel = MapSharingLevelToPrivacyLevel(t);

                if (t == "A")
                {
                    participant.PersonalIdNumber = null;
                    participant.Hmo = null;
                    participant.Phone1 = null;
                    participant.Phone2 = null;
                    participant.FixedMedications = null;
                    participant.EmergencyContactName = null;
                    participant.EmergencyContactPhone = null;
                    participant.MicConsent = false;
                    participant.CameraConsent = false;
                }
                else if (t == "B")
                {
                    participant.MicConsent = false;
                    participant.CameraConsent = false;
                }

                _db.SaveChanges();
            }

            LogAction(pid.Value, "DowngradePrivacyLevel", $"To={t}");
            return RedirectToAction("RegisterMini");
        }

        [HttpPost]
        public IActionResult DeleteMyData()
        {
            var pid = GetParticipantId();
            if (pid == null)
                return RedirectToAction("Login", "Experiment");

            // Session clear (רק מה שיש אצלנו)
            HttpContext.Session.Remove("PersonalIdNumber");
            HttpContext.Session.Remove("Hmo");
            HttpContext.Session.Remove("Phone1");
            HttpContext.Session.Remove("Phone2");
            HttpContext.Session.Remove("FixedMedications");
            HttpContext.Session.Remove("EmergencyContactName");
            HttpContext.Session.Remove("EmergencyContactPhone");
            HttpContext.Session.SetString("MicConsent", "false");
            HttpContext.Session.SetString("CameraConsent", "false");
            HttpContext.Session.Remove("SharingLevel");
            HttpContext.Session.SetString("IsLoggedIn", "false");

            // DB clear
            var participant = GetCurrentParticipant(pid.Value);
            if (participant != null)
            {
                participant.PersonalIdNumber = null;
                participant.Hmo = null;
                participant.Phone1 = null;
                participant.Phone2 = null;
                participant.FixedMedications = null;
                participant.EmergencyContactName = null;
                participant.EmergencyContactPhone = null;
                participant.MicConsent = false;
                participant.CameraConsent = false;
                participant.CurrentPrivacyLevel = null;

                _db.SaveChanges();
            }

            LogAction(pid.Value, "DeleteMyData", "All personal data cleared");
            return RedirectToAction("Intro", "Prototype");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            var pid = GetParticipantId();

            if (pid != null)
            {
                var openSession = _db.ParticipantSessions
                    .Where(s => s.ParticipantId == pid.Value && s.EndedAtUtc == null)
                    .OrderByDescending(s => s.StartedAtUtc)
                    .FirstOrDefault();

                if (openSession != null)
                {
                    openSession.EndedAtUtc = DateTime.UtcNow;
                    _db.SaveChanges();
                }

                LogAction(pid.Value, "Logout");
            }

            // ✅ לא עושים Session.Clear() כדי לא למחוק נתוני שירותים שנשמרו בסשן (תזכורות וכו')
            // מנקים רק נתונים שקשורים להתחברות/תצוגה
            HttpContext.Session.SetString("IsLoggedIn", "false");
            HttpContext.Session.Remove("FirstName");
            HttpContext.Session.Remove("ParticipantId");


            return RedirectToAction("Login", "Experiment");
        }

    }
}
