using System;
using System.Collections.Generic;
using System.Linq;
using ElderSharingPrototype.Data;
using ElderSharingPrototype.Helpers;
using ElderSharingPrototype.Models;
using ElderSharingPrototype.Models.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ElderSharingPrototype.Controllers
{
    public class HealthController : Controller
    {
        private readonly AppDbContext _db;

        public HealthController(AppDbContext db)
        {
            _db = db;
        }

        // A / B / C
        private string Level => (HttpContext.Session.GetString("SharingLevel") ?? "A").Trim().ToUpperInvariant();

        private int? GetParticipantId() => HttpContext.Session.GetInt32("ParticipantId");

        private IActionResult Blocked(string serviceName, string targetLevel)
        {
            TempData["BlockedService"] = serviceName;
            TempData["TargetLevel"] = targetLevel;
            return RedirectToAction("Services", "Prototype");
        }

        // ------------------------------------------------------------
        // Helpers - Medications
        // ------------------------------------------------------------
        private Participant? GetCurrentParticipant()
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return null;
            return _db.Participants.FirstOrDefault(p => p.Id == pid.Value);
        }

        private List<string> GetUserMedicationList()
        {
            string fixedMeds;

            var participant = GetCurrentParticipant();
            fixedMeds = participant?.FixedMedications ?? "";

            if (string.IsNullOrWhiteSpace(fixedMeds))
                return new List<string>();

            return fixedMeds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> GetMedicationCatalog()
        {
            return new List<string>
            {
                "אקמול", "אדויל", "אספירין", "אומפרדקס", "אומפרזול", "אנטיהיסטמין",
                "ברופן", "בטאקורטן", "ביסופרולול", "ברזל", "בודיקורט",
                "גלוקופאג׳", "גסטרו", "ג׳לוסיל", "גאבאפנטין", "גינרה",
                "דקסמול", "דקסמול קולד", "דופסטון", "דיאמיקרון", "דוקסילין",
                "הידרוכלורותיאזיד", "הידרוקסיזין", "הפארין",
                "ויטמין D", "ויטמין B12", "וולטרן", "ורמוקס",
                "זודורם", "זירקט", "זינאט",
                "חומצה פולית",
                "טאמסולין", "טמסולוסין", "טראמדקס", "טוביאז",
                "יאז", "יוויטל",
                "כדורי ברזל", "כדורי סידן",
                "לוסק", "לוריוון", "לורטאדין", "לבותרוקסין", "לנטוס",
                "מטפורמין", "מוקסיפן", "מגנזיום", "מיקרופירין", "מונולונג",
                "נורופן", "נקסיום", "נורמלקס", "נורמיטן",
                "סטטינים", "סימוביל", "סימבסטטין", "סולפיריד", "סולפיד",
                "פראמין", "פרדניזון", "פרדניזולון", "פלביקס", "פנרגן",
                "ציפרודקס", "ציפרופלוקסצין", "ציפרודין", "ציפרלקס", "צנטרום",
                "קונקור", "קלונקס", "קלקסן", "קווינפריל", "קולכיצין",
                "ריטלין", "רוקסט", "רניטידין", "רמרון",
                "תירוקסין", "תמיפלו"
            };
        }

        // ------------------------------------------------------------
        // רמה 1 (A) – שירותים כלליים
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult CheckupRecommendations()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AudioAlerts()
        {
            return View();
        }

        // ------------------------------------------------------------
        // ניהול תרופות + תזכורות תרופות (A/B/C)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult MedicationList()
        {
            return RedirectToAction(nameof(MedicationReminders), new { mode = "detailed" });
        }

        [HttpGet]
        public IActionResult MedicationReminders(string? mode)
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var level = Level;
            var normalizedMode = (mode ?? "").Trim().ToLowerInvariant();

            bool basicMode = normalizedMode == "basic";
            bool detailedMode = normalizedMode == "detailed";

            if (!basicMode && !detailedMode)
            {
                if (level == "A")
                    basicMode = true;
                else
                    detailedMode = true;
            }

            if (level == "A")
            {
                basicMode = true;
                detailedMode = false;
            }

            var reminderModeToShow = basicMode ? "basic" : "detailed";

            var reminders = _db.ReminderItems
                .Where(x =>
                    x.ParticipantId == pid.Value &&
                    x.Type == "Medication" &&
                    x.MedicationMode == reminderModeToShow)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToList();

            var vm = new MedicationCenterViewModel
            {
                Level = level,
                MedicationList = GetUserMedicationList(),
                MedicationCatalog = GetMedicationCatalog(),
                Reminders = reminders.Select(x => new ReminderItem
                {
                    Id = x.Id,
                    Type = x.Type,
                    Title = x.Title,
                    Time = x.Time,
                    Notes = x.Notes ?? "",
                    MedicationName = x.MedicationName
                }).ToList()
            };

            if (basicMode)
                return View("MedicationRemindersLevel1", vm);

            return View("MedicationReminders", vm);
        }

        [HttpPost]
        public IActionResult AddMedicationToList(string name)
        {
            if (Level != "B" && Level != "C")
                return Blocked("ניהול תרופות ותזכורות", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var meds = GetUserMedicationList();
            var newName = (name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                TempData["MedListError"] = "נא לבחור תרופה מהרשימה להוספה.";
                return RedirectToAction(nameof(MedicationReminders), new { mode = "detailed" });
            }

            var catalog = GetMedicationCatalog();
            var inCatalog = catalog.Any(m => string.Equals(m, newName, StringComparison.OrdinalIgnoreCase));
            if (!inCatalog)
            {
                TempData["MedListError"] = "התרופה שנבחרה אינה קיימת בקטלוג. נא לבחור מהרשימה.";
                return RedirectToAction(nameof(MedicationReminders), new { mode = "detailed" });
            }

            if (meds.Any(m => string.Equals(m, newName, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["MedListError"] = "התרופה כבר קיימת ברשימה שלך.";
                return RedirectToAction(nameof(MedicationReminders), new { mode = "detailed" });
            }

            meds.Add(newName);
            meds = meds.OrderBy(x => x).ToList();

            var participant = GetCurrentParticipant();
            if (participant != null)
            {
                participant.FixedMedications = string.Join(", ", meds);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(MedicationReminders), new { mode = "detailed" });
        }

        [HttpPost]
        public IActionResult RemoveMedicationFromList(string name)
        {
            if (Level != "B" && Level != "C")
                return Blocked("ניהול תרופות ותזכורות", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var meds = GetUserMedicationList();
            var toRemove = (name ?? "").Trim();

            meds = meds
                .Where(m => !string.Equals(m, toRemove, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var participant = GetCurrentParticipant();
            if (participant != null)
            {
                participant.FixedMedications = string.Join(", ", meds);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(MedicationReminders), new { mode = "detailed" });
        }

        [HttpPost]
        public IActionResult AddMedicationReminder(string? medicationName, string time, string? notes, string? mode)
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var level = Level;
            var normalizedMode = (mode ?? "").Trim().ToLowerInvariant();

            bool isDetailedMode = normalizedMode == "detailed" && (level == "B" || level == "C");
            bool isBasicMode = !isDetailedMode;

            var userMeds = GetUserMedicationList();

            if (string.IsNullOrWhiteSpace(time))
            {
                TempData["ReminderError"] = "נא לבחור שעה.";
                return RedirectToAction(nameof(MedicationReminders), new { mode = isDetailedMode ? "detailed" : "basic" });
            }

            if (isDetailedMode)
            {
                if (userMeds == null || userMeds.Count == 0)
                {
                    TempData["ReminderError"] = "אין תרופות שמורות ברשימה שלך. הוסיפי קודם תרופה לרשימה.";
                    return RedirectToAction(nameof(MedicationReminders), new { mode = "detailed" });
                }

                if (string.IsNullOrWhiteSpace(medicationName))
                {
                    TempData["ReminderError"] = "נא לבחור תרופה מהרשימה שלך.";
                    return RedirectToAction(nameof(MedicationReminders), new { mode = "detailed" });
                }

                var chosenMedication = medicationName.Trim();
                var exists = userMeds.Any(m => string.Equals(m, chosenMedication, StringComparison.OrdinalIgnoreCase));
                if (!exists)
                {
                    TempData["ReminderError"] = "התרופה שנבחרה אינה נמצאת ברשימה שלך. נא לבחור תרופה מהרשימה.";
                    return RedirectToAction(nameof(MedicationReminders), new { mode = "detailed" });
                }
            }

            string title;
            string? medNameToSave;
            string medicationModeToSave;

            if (isDetailedMode)
            {
                var chosen = medicationName!.Trim();
                title = $"תזכורת לנטילת: {chosen}";
                medNameToSave = chosen;
                medicationModeToSave = "detailed";
            }
            else
            {
                title = "תזכורת כללית ללקיחת תרופות";
                medNameToSave = null;
                medicationModeToSave = "basic";
            }

            _db.ReminderItems.Add(new ReminderItemEntity
            {
                ParticipantId = pid.Value,
                Type = "Medication",
                Title = title,
                Time = time.Trim(),
                Notes = (notes ?? "").Trim(),
                MedicationName = medNameToSave,
                MedicationMode = medicationModeToSave
            });

            _db.SaveChanges();

            return RedirectToAction(nameof(MedicationReminders), new { mode = isDetailedMode ? "detailed" : "basic" });
        }

        [HttpPost]
        public IActionResult DeleteMedicationReminder(Guid id, string? mode)
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var item = _db.ReminderItems.FirstOrDefault(x => x.Id == id && x.ParticipantId == pid.Value);
            if (item != null)
            {
                _db.ReminderItems.Remove(item);
                _db.SaveChanges();
            }

            var normalizedMode = (mode ?? "").Trim().ToLowerInvariant();
            return RedirectToAction(nameof(MedicationReminders), new { mode = normalizedMode == "detailed" ? "detailed" : "basic" });
        }

        [HttpGet]
        public IActionResult PharmacyContact(string? city)
        {
            if (Level != "C")
                return Blocked("עדכון מלאי וקשר עם בית המרקחת", "C");

            var allBranches = new List<PharmacyBranchViewModel>
    {
        new PharmacyBranchViewModel
        {
            City = "באר שבע",
            BranchName = "סופר-פארם גרנד קניון",
            PhoneNumber = "08-6234567",
            Address = "דרך חברון 21, באר שבע"
        },
        new PharmacyBranchViewModel
        {
            City = "באר שבע",
            BranchName = "בית מרקחת כללית מרכז הנגב",
            PhoneNumber = "08-6345678",
            Address = "ויצמן 15, באר שבע"
        },
        new PharmacyBranchViewModel
        {
            City = "אשקלון",
            BranchName = "סופר-פארם אשקלון מול הים",
            PhoneNumber = "08-6745678",
            Address = "בן גוריון 10, אשקלון"
        },
        new PharmacyBranchViewModel
        {
            City = "אשקלון",
            BranchName = "בית מרקחת מכבי אשקלון",
            PhoneNumber = "08-6856789",
            Address = "ההסתדרות 5, אשקלון"
        },
        new PharmacyBranchViewModel
        {
            City = "תל אביב",
            BranchName = "סופר-פארם דיזנגוף סנטר",
            PhoneNumber = "03-6123456",
            Address = "דיזנגוף 50, תל אביב"
        },
        new PharmacyBranchViewModel
        {
            City = "תל אביב",
            BranchName = "בית מרקחת כללית אבן גבירול",
            PhoneNumber = "03-6234567",
            Address = "אבן גבירול 110, תל אביב"
        },
        new PharmacyBranchViewModel
        {
            City = "קריית גת",
            BranchName = "סופר-פארם קניון לב העיר",
            PhoneNumber = "08-6543210",
            Address = "מלכי ישראל 178, קריית גת"
        },
        new PharmacyBranchViewModel
        {
            City = "קריית גת",
            BranchName = "בית מרקחת מאוחדת קריית גת",
            PhoneNumber = "08-6654321",
            Address = "שדרות לכיש 7, קריית גת"
        }
    };

            var cities = new List<string>
    {
        "באר שבע",
        "אשקלון",
        "תל אביב",
        "קריית גת"
    };

            var selectedCity = (city ?? "").Trim();

            var filteredBranches = string.IsNullOrWhiteSpace(selectedCity)
                ? new List<PharmacyBranchViewModel>()
                : allBranches.Where(b => b.City == selectedCity).ToList();

            ViewBag.Cities = cities;
            ViewBag.SelectedCity = selectedCity;
            ViewBag.Branches = filteredBranches;

            return View();
        }

        // ------------------------------------------------------------
        // תזכורות מדדים (A/B/C) - שמירה ל-DB
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult VitalsReminders()
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var list = _db.ReminderItems
                .Where(x => x.ParticipantId == pid.Value && x.Type == "Vitals")
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToList();

            var vm = list.Select(x => new ReminderItem
            {
                Id = x.Id,
                Type = x.Type,
                Title = x.Title,
                Time = x.Time,
                Notes = x.Notes ?? ""
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        public IActionResult AddVitalsReminder(string vitalType, string time)
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            if (string.IsNullOrWhiteSpace(vitalType))
                TempData["VitalsErr"] = "נא לבחור סוג מדד.";
            if (string.IsNullOrWhiteSpace(time))
                TempData["VitalsErr"] = "נא לבחור שעה.";

            if (TempData.ContainsKey("VitalsErr"))
                return RedirectToAction(nameof(VitalsReminders));

            _db.ReminderItems.Add(new ReminderItemEntity
            {
                ParticipantId = pid.Value,
                Type = "Vitals",
                Title = vitalType.Trim(),
                Time = time.Trim(),
                Notes = ""
            });
            _db.SaveChanges();

            return RedirectToAction(nameof(VitalsReminders));
        }

        [HttpPost]
        public IActionResult DeleteVitalsReminder(Guid id)
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var item = _db.ReminderItems.FirstOrDefault(x => x.Id == id && x.ParticipantId == pid.Value);
            if (item != null)
            {
                _db.ReminderItems.Remove(item);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(VitalsReminders));
        }

        // ------------------------------------------------------------
        // מדידות (B/C)
        // B: חד פעמי בלבד (לא נשמר)
        // C: נשמר ב-DB ומופיע בהיסטוריה
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult Measurements(string? mode)
        {
            if (Level != "B" && Level != "C")
                return Blocked("הזנת מדידות", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var normalizedMode = (mode ?? "").Trim().ToLowerInvariant();

            bool oneTimeMode = normalizedMode == "onetime";
            bool historyMode = normalizedMode == "history";

            // ברירת מחדל:
            // משתמש רמה B -> חד פעמי
            // משתמש רמה C -> היסטוריה
            if (!oneTimeMode && !historyMode)
            {
                if (Level == "B")
                    oneTimeMode = true;
                else
                    historyMode = true;
            }

            // משתמש רמה B לא יכול להיכנס למסך היסטוריה
            if (Level == "B")
            {
                oneTimeMode = true;
                historyMode = false;
            }

            ViewBag.Level = Level;
            ViewBag.MeasurementsMode = oneTimeMode ? "onetime" : "history";

            // מסך חד־פעמי: אין היסטוריה בכלל
            if (oneTimeMode)
                return View(new List<VitalMeasurementEntity>());

            // מסך היסטוריה: רק לרמה C
            var data = _db.VitalMeasurements
                .AsNoTracking()
                .Where(x => x.ParticipantId == pid.Value)
                .OrderByDescending(x => x.MeasuredAt)
                .ToList();

            return View(data);
        }

        [HttpPost]
        public IActionResult PullDeviceMeasurements(string? scenario, string? mode)
        {
            if (Level != "B" && Level != "C")
                return Json(new { ok = false, message = "Service available only for Level B/C" });

            var pid = GetParticipantId();
            if (!pid.HasValue)
                return Json(new { ok = false, message = "Not logged in" });

            var normalizedMode = (mode ?? "").Trim().ToLowerInvariant();

            bool oneTimeMode = normalizedMode == "onetime";
            bool historyMode = normalizedMode == "history";

            if (!oneTimeMode && !historyMode)
            {
                if (Level == "B")
                    oneTimeMode = true;
                else
                    historyMode = true;
            }

            // משתמש ברמה B לא יכול לשמור היסטוריה
            if (Level == "B")
            {
                oneTimeMode = true;
                historyMode = false;
            }

            var sc = (scenario ?? "normal").Trim().ToLowerInvariant();

            int systolic, diastolic, sugar;

            if (sc == "high")
            {
                systolic = Random.Shared.Next(150, 185);
                diastolic = Random.Shared.Next(95, 115);
                sugar = Random.Shared.Next(190, 260);
            }
            else
            {
                systolic = Random.Shared.Next(105, 136);
                diastolic = Random.Shared.Next(65, 86);
                sugar = Random.Shared.Next(80, 141);
            }

            var nowLocal = DateTime.Now;
            var nowUtc = DateTime.UtcNow;

            if (oneTimeMode)
            {
                return Json(new
                {
                    ok = true,
                    mode = "oneTime",
                    measuredAt = nowLocal.ToString("dd/MM/yyyy HH:mm"),
                    bloodPressure = new { systolic, diastolic },
                    sugar = new { mgDl = sugar }
                });
            }

            _db.VitalMeasurements.Add(new VitalMeasurementEntity
            {
                ParticipantId = pid.Value,
                Kind = "BloodPressure",
                Systolic = systolic,
                Diastolic = diastolic,
                Notes = "התקבל אוטומטית מהמכשיר (WiFi) – דמו",
                MeasuredAt = nowUtc
            });

            _db.VitalMeasurements.Add(new VitalMeasurementEntity
            {
                ParticipantId = pid.Value,
                Kind = "Sugar",
                SugarMgDl = sugar,
                Notes = "התקבל אוטומטית מהמכשיר (WiFi) – דמו",
                MeasuredAt = nowUtc
            });

            _db.SaveChanges();

            return Json(new
            {
                ok = true,
                mode = "saved",
                measuredAt = nowLocal.ToString("dd/MM/yyyy HH:mm"),
                bloodPressure = new { systolic, diastolic },
                sugar = new { mgDl = sugar }
            });
        }

        [HttpPost]
        public IActionResult DeleteMeasurementsHistory()
        {
            if (Level != "C")
                return Blocked("מחיקת היסטוריית מדדים", "C");

            var pid = GetParticipantId();
            if (!pid.HasValue)
                return RedirectToAction("Login", "Experiment");

            var items = _db.VitalMeasurements
                .Where(x => x.ParticipantId == pid.Value)
                .ToList();

            if (items.Any())
            {
                _db.VitalMeasurements.RemoveRange(items);
                _db.SaveChanges();
            }

            TempData["MeasurementsDeletedOk"] = "היסטוריית המדדים נמחקה בהצלחה.";
            return RedirectToAction(nameof(Measurements), new { mode = "history" });
        }

        // ------------------------------------------------------------
        // זימון תור – רמה B וגם C (DB)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult AppointmentRequest()
        {
            if (Level != "B" && Level != "C")
                return Blocked("זימון תור לרופא", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var list = _db.AppointmentRequests
                .Where(x => x.ParticipantId == pid.Value)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToList();

            ViewBag.Requests = list.Select(x => new AppointmentRequest
            {
                Id = x.Id,
                TreatmentArea = x.TreatmentArea,
                Reason = x.Reason,
                PreferredDate = x.PreferredDate,
                PreferredTime = x.PreferredTime,
                Notes = x.Notes ?? ""
            }).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult AddAppointment(string treatmentArea, string reason, string preferredDate, string preferredTime, string? notes)
        {
            if (Level != "B" && Level != "C")
                return Blocked("זימון תור לרופא", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            if (string.IsNullOrWhiteSpace(treatmentArea))
                ModelState.AddModelError("treatmentArea", "נא לבחור תחום טיפול.");
            if (string.IsNullOrWhiteSpace(reason))
                ModelState.AddModelError("reason", "נא לציין סיבה.");
            if (string.IsNullOrWhiteSpace(preferredDate))
                ModelState.AddModelError("preferredDate", "נא לבחור תאריך.");
            if (string.IsNullOrWhiteSpace(preferredTime))
                ModelState.AddModelError("preferredTime", "נא לבחור שעה.");

            if (!ModelState.IsValid)
                return RedirectToAction(nameof(AppointmentRequest));

            _db.AppointmentRequests.Add(new AppointmentRequestEntity
            {
                ParticipantId = pid.Value,
                TreatmentArea = treatmentArea.Trim(),
                Reason = reason.Trim(),
                PreferredDate = preferredDate.Trim(),
                PreferredTime = preferredTime.Trim(),
                Notes = (notes ?? "").Trim()
            });
            _db.SaveChanges();

            return RedirectToAction(nameof(AppointmentRequest));
        }

        [HttpPost]
        public IActionResult DeleteAppointment(Guid id)
        {
            if (Level != "B" && Level != "C")
                return Blocked("זימון תור לרופא", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var item = _db.AppointmentRequests.FirstOrDefault(x => x.Id == id && x.ParticipantId == pid.Value);
            if (item != null)
            {
                _db.AppointmentRequests.Remove(item);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(AppointmentRequest));
        }

        // ------------------------------------------------------------
        // B/C – פרטי איש קשר (DB)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult EmergencyContact()
        {
            if (Level != "B" && Level != "C")
                return Blocked("פרטי איש קשר ויצירת קשר טקסטואלי במקרה חירום", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var contact = _db.EmergencyContacts.FirstOrDefault(x => x.ParticipantId == pid.Value);
            var draft = _db.EmergencyTextDrafts.FirstOrDefault(x => x.ParticipantId == pid.Value);

            var vm = new EmergencyContactTextViewModel
            {
                FullName = contact?.FullName ?? "",
                PhoneNumber = contact?.PhoneNumber ?? "",
                Relationship = contact?.Relationship ?? "",
                DefaultMessage = contact?.DefaultMessage ?? "",
                Address = draft?.Address ?? "",
                MessageText = draft?.MessageText ?? ""
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult EmergencyContact(EmergencyContactTextViewModel model)
        {
            if (Level != "B" && Level != "C")
                return Blocked("פרטי איש קשר ויצירת קשר טקסטואלי במקרה חירום", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            if (!ModelState.IsValid)
                return View(model);

            var contact = _db.EmergencyContacts.FirstOrDefault(x => x.ParticipantId == pid.Value);
            if (contact == null)
            {
                contact = new EmergencyContactEntity
                {
                    ParticipantId = pid.Value
                };
                _db.EmergencyContacts.Add(contact);
            }

            contact.FullName = (model.FullName ?? "").Trim();
            contact.PhoneNumber = (model.PhoneNumber ?? "").Trim();
            contact.Relationship = (model.Relationship ?? "").Trim();
            contact.UpdatedAtUtc = DateTime.UtcNow;

            var draft = _db.EmergencyTextDrafts.FirstOrDefault(x => x.ParticipantId == pid.Value);
            if (draft == null)
            {
                draft = new EmergencyTextDraftEntity
                {
                    ParticipantId = pid.Value
                };
                _db.EmergencyTextDrafts.Add(draft);
            }

            draft.Address = (model.Address ?? "").Trim();
            draft.MessageText = (model.MessageText ?? "").Trim();
            draft.UpdatedAtUtc = DateTime.UtcNow;

            _db.SaveChanges();

            TempData["SavedOk"] = "הפרטים נשמרו בהצלחה.";
            return RedirectToAction(nameof(EmergencyContact));
        }

        // ------------------------------------------------------------
        // B/C – קשר טקסטואלי בחירום (DB)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult EmergencyText()
        {
            return RedirectToAction(nameof(EmergencyContact));
        }

        [HttpPost]
        public IActionResult EmergencyText(EmergencyTextViewModel model)
        {
            return RedirectToAction(nameof(EmergencyContact));
        }

        public class TeleVisitScheduleDto
        {
            public string? VisitType { get; set; }
            public string? PreferredDate { get; set; }
            public string? PreferredTime { get; set; }
            public string? Reason { get; set; }
        }

        [HttpPost]
        public IActionResult SendEmergencyText(EmergencyContactTextViewModel model)
        {
            if (Level != "B" && Level != "C")
                return Blocked("פרטי איש קשר ויצירת קשר טקסטואלי במקרה חירום", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            if (string.IsNullOrWhiteSpace(model.FullName))
                ModelState.AddModelError(nameof(model.FullName), "נא להזין שם איש קשר.");

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                ModelState.AddModelError(nameof(model.PhoneNumber), "נא להזין מספר טלפון.");

            if (string.IsNullOrWhiteSpace(model.MessageText))
                ModelState.AddModelError(nameof(model.MessageText), "נא לבחור הודעת חירום.");

            if (!ModelState.IsValid)
                return View("EmergencyContact", model);

            var contact = _db.EmergencyContacts.FirstOrDefault(x => x.ParticipantId == pid.Value);
            if (contact == null)
            {
                contact = new EmergencyContactEntity
                {
                    ParticipantId = pid.Value
                };
                _db.EmergencyContacts.Add(contact);
            }

            contact.FullName = (model.FullName ?? "").Trim();
            contact.PhoneNumber = (model.PhoneNumber ?? "").Trim();
            contact.Relationship = (model.Relationship ?? "").Trim();
            contact.UpdatedAtUtc = DateTime.UtcNow;

            var draft = _db.EmergencyTextDrafts.FirstOrDefault(x => x.ParticipantId == pid.Value);
            if (draft == null)
            {
                draft = new EmergencyTextDraftEntity
                {
                    ParticipantId = pid.Value
                };
                _db.EmergencyTextDrafts.Add(draft);
            }

            draft.Address = (model.Address ?? "").Trim();
            draft.MessageText = (model.MessageText ?? "").Trim();
            draft.LastSendSucceededDemo = true;
            draft.UpdatedAtUtc = DateTime.UtcNow;

            _db.SaveChanges();

            TempData["MsgOk"] = "ההודעה נשלחה (דמו).";
            return RedirectToAction(nameof(EmergencyContact));
        }

        [HttpPost]
        public IActionResult ScheduleTeleVisit([FromBody] TeleVisitScheduleDto dto)
        {
            if (Level != "C")
                return Json(new { ok = false, message = "Service available only for Level C" });

            var pid = GetParticipantId();
            if (!pid.HasValue)
                return Json(new { ok = false, message = "Not logged in" });

            var visitType = (dto.VisitType ?? "video").Trim().ToLowerInvariant();
            if (visitType != "video" && visitType != "phone")
                return Json(new { ok = false, message = "Invalid visitType" });

            var date = (dto.PreferredDate ?? "").Trim();
            var time = (dto.PreferredTime ?? "").Trim();
            var reason = (dto.Reason ?? "").Trim();

            if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
                return Json(new { ok = false, message = "Date/time is required" });

            var entity = new AppointmentRequestEntity
            {
                ParticipantId = pid.Value,
                TreatmentArea = "רופא (TeleVisit)",
                Reason = reason,
                PreferredDate = date,
                PreferredTime = time,
                Notes = visitType,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.AppointmentRequests.Add(entity);
            _db.SaveChanges();

            return Json(new { ok = true, id = entity.Id });
        }

        [HttpPost]
        public IActionResult CancelTeleVisit(Guid id)
        {
            if (Level != "C")
                return Blocked("תור טלפוני / תור וידיאו", "C");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var item = _db.AppointmentRequests.FirstOrDefault(x => x.Id == id && x.ParticipantId == pid.Value);
            if (item != null)
            {
                _db.AppointmentRequests.Remove(item);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(TeleVisit));
        }

        // ------------------------------------------------------------
        // C – תור טלפוני / וידיאו (DB)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult TeleVisit()
        {
            if (Level != "C")
                return Blocked("תור טלפוני / תור וידיאו", "C");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var list = _db.AppointmentRequests
                .Where(x => x.ParticipantId == pid.Value)
                .AsNoTracking()
                .ToList();

            var upcoming = new List<AppointmentRequestEntity>();
            foreach (var a in list)
            {
                DateTime dt;
                var dateStr = (a.PreferredDate ?? "").Trim();
                var timeStr = (a.PreferredTime ?? "").Trim();

                if (DateTime.TryParse($"{dateStr} {timeStr}", out dt))
                {
                    if (dt >= DateTime.Now.AddMinutes(-5))
                        upcoming.Add(a);
                }
                else
                {
                    upcoming.Add(a);
                }
            }

            ViewBag.Upcoming = upcoming
                .OrderBy(x => x.PreferredDate)
                .ThenBy(x => x.PreferredTime)
                .ToList();

            return View();
        }

        // ------------------------------------------------------------
        // C – קשר חזותי בחירום (DB)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult EmergencyVideo()
        {
            if (Level != "C")
                return Blocked("פרטי איש קשר ויצירת קשר חזותי בחירום", "C");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var contact = _db.EmergencyContacts.FirstOrDefault(x => x.ParticipantId == pid.Value);
            var video = _db.EmergencyVideos.FirstOrDefault(x => x.ParticipantId == pid.Value);

            var vm = new EmergencyVideoViewModel
            {
                FullName = contact?.FullName ?? "",
                PhoneNumber = contact?.PhoneNumber ?? "",
                VideoUrl = video?.VideoUrl ?? "",
                Notes = video?.Notes ?? ""
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult EmergencyVideo(EmergencyVideoViewModel model)
        {
            if (Level != "C")
                return Blocked("פרטי איש קשר ויצירת קשר חזותי בחירום", "C");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            if (!ModelState.IsValid)
                return View(model);

            var row = _db.EmergencyVideos.FirstOrDefault(x => x.ParticipantId == pid.Value);
            if (row == null)
            {
                row = new EmergencyVideoEntity
                {
                    ParticipantId = pid.Value
                };
                _db.EmergencyVideos.Add(row);
            }

            row.VideoUrl = (model.VideoUrl ?? "").Trim();
            row.Notes = (model.Notes ?? "").Trim();
            row.UpdatedAtUtc = DateTime.UtcNow;

            _db.SaveChanges();

            TempData["SavedOk"] = "פרטי הווידאו נשמרו (דמו).";
            return RedirectToAction(nameof(EmergencyVideo));
        }

        [HttpGet]
        public IActionResult DueReminders()
        {
            var pid = GetParticipantId();
            if (!pid.HasValue)
                return Json(new { ok = false, message = "Not logged in" });

            var nowLocal = DateTime.Now;
            var hhmm = nowLocal.ToString("HH:mm");

            var minuteKey = nowLocal.ToString("yyyy-MM-dd HH:mm");
            var lastKey = HttpContext.Session.GetString("LastReminderMinuteKey");

            if (lastKey == minuteKey)
            {
                return Json(new { ok = true, now = hhmm, due = Array.Empty<object>() });
            }

            var due = _db.Set<ReminderItemEntity>()
                .AsNoTracking()
                .Where(r => r.ParticipantId == pid.Value && r.Time == hhmm)
                .OrderBy(r => r.Type)
                .Select(r => new
                {
                    id = r.Id,
                    type = r.Type,
                    title = r.Title,
                    time = r.Time,
                    medicationName = r.MedicationName,
                    notes = r.Notes
                })
                .ToList();

            if (due.Any())
            {
                HttpContext.Session.SetString("LastReminderMinuteKey", minuteKey);
            }

            return Json(new { ok = true, now = hhmm, due });
        }
    }
}