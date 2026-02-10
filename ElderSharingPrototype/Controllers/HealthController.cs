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
        // Helpers - Medications (נשאר כמו שהיה אצלך: FixedMedications ב-Participant)
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
        // רמה 1 (A) – שירותים כלליים (פתוח לכולם)
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
        // ניהול רשימת תרופות קבועות (B/C) - נשמר ב-Participant.FixedMedications
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult MedicationList()
        {
            if (Level != "B" && Level != "C")
                return Blocked("ניהול רשימת תרופות קבועות", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            ViewBag.Level = Level;
            ViewBag.MedCatalog = GetMedicationCatalog();

            var meds = GetUserMedicationList();
            return View(meds);
        }

        [HttpPost]
        public IActionResult RemoveMedicationFromList(string name)
        {
            if (Level != "B" && Level != "C")
                return Blocked("ניהול רשימת תרופות קבועות", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var meds = GetUserMedicationList();
            var toRemove = (name ?? "").Trim();

            meds = meds.Where(m => !string.Equals(m, toRemove, StringComparison.OrdinalIgnoreCase)).ToList();

            var participant = GetCurrentParticipant();
            if (participant != null)
            {
                participant.FixedMedications = string.Join(", ", meds);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(MedicationList));
        }

        [HttpPost]
        public IActionResult AddMedicationToList(string name)
        {
            if (Level != "B" && Level != "C")
                return Blocked("ניהול רשימת תרופות קבועות", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var meds = GetUserMedicationList();
            var newName = (name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                TempData["MedListError"] = "נא לבחור תרופה מהרשימה להוספה.";
                return RedirectToAction(nameof(MedicationList));
            }

            var catalog = GetMedicationCatalog();
            var inCatalog = catalog.Any(m => string.Equals(m, newName, StringComparison.OrdinalIgnoreCase));
            if (!inCatalog)
            {
                TempData["MedListError"] = "התרופה שנבחרה אינה קיימת בקטלוג. נא לבחור מהרשימה.";
                return RedirectToAction(nameof(MedicationList));
            }

            if (meds.Any(m => string.Equals(m, newName, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["MedListError"] = "התרופה כבר קיימת ברשימה שלך.";
                return RedirectToAction(nameof(MedicationList));
            }

            meds.Add(newName);
            meds = meds.OrderBy(x => x).ToList();

            var participant = GetCurrentParticipant();
            if (participant != null)
            {
                participant.FixedMedications = string.Join(", ", meds);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(MedicationList));
        }

        // ------------------------------------------------------------
        // תזכורות תרופות (A/B/C) - שמירה ל-DB
        // A: כללית, B/C: לפי שם תרופה מהרשימה
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult MedicationReminders()
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            ViewBag.Level = Level;
            ViewBag.UserMeds = GetUserMedicationList();

            var list = _db.ReminderItems
                .Where(x => x.ParticipantId == pid.Value && x.Type == "Medication")
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToList();

            // משתמשים ב-ReminderItem הקיים שלך ל-View (אם ה-View שלך מצפה לזה)
            var vm = list.Select(x => new ReminderItem
            {
                Id = x.Id,
                Type = x.Type,
                Title = x.Title,
                Time = x.Time,
                Notes = x.Notes ?? "",
                MedicationName = x.MedicationName
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        public IActionResult AddMedicationReminder(string? medicationName, string time, string? notes)
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var level = Level;
            ViewBag.Level = level;

            var userMeds = GetUserMedicationList();
            ViewBag.UserMeds = userMeds;

            if (string.IsNullOrWhiteSpace(time))
                ModelState.AddModelError("time", "נא לבחור שעה.");

            if (level == "B" || level == "C")
            {
                if (userMeds == null || userMeds.Count == 0)
                {
                    ModelState.AddModelError("medicationName", "אין תרופות שמורות ברשימה שלך. חזרי למסך 'ניהול רשימת תרופות קבועות' והוסיפי תרופות.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(medicationName))
                        ModelState.AddModelError("medicationName", "נא לבחור תרופה מהרשימה שלך.");
                    else
                    {
                        var chosen = medicationName.Trim();
                        var exists = userMeds.Any(m => string.Equals(m, chosen, StringComparison.OrdinalIgnoreCase));
                        if (!exists)
                            ModelState.AddModelError("medicationName", "התרופה שנבחרה אינה נמצאת ברשימה שלך. נא לבחור תרופה מהרשימה.");
                    }
                }
            }

            if (!ModelState.IsValid)
                return RedirectToAction(nameof(MedicationReminders));

            string title;
            string? medNameToSave;

            if (level == "B" || level == "C")
            {
                var chosen = medicationName!.Trim();
                title = $"תזכורת לנטילת: {chosen}";
                medNameToSave = chosen;
            }
            else
            {
                title = "תזכורת כללית ללקיחת תרופות";
                medNameToSave = null;
            }

            _db.ReminderItems.Add(new ReminderItemEntity
            {
                ParticipantId = pid.Value,
                Type = "Medication",
                Title = title,
                Time = time.Trim(),
                Notes = (notes ?? "").Trim(),
                MedicationName = medNameToSave
            });
            _db.SaveChanges();

            return RedirectToAction(nameof(MedicationReminders));
        }

        [HttpPost]
        public IActionResult DeleteMedicationReminder(Guid id)
        {
            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var item = _db.ReminderItems.FirstOrDefault(x => x.Id == id && x.ParticipantId == pid.Value);
            if (item != null)
            {
                _db.ReminderItems.Remove(item);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(MedicationReminders));
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
        public IActionResult Measurements()
        {
            if (Level != "B" && Level != "C")
                return Blocked("הזנת מדידות", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            ViewBag.Level = Level;

            // ברמה B אין היסטוריה בכלל (הצגה חד פעמית בלבד דרך JS)
            if (Level == "B")
                return View(new List<VitalMeasurementEntity>());

            // רמה C: להביא היסטוריה מה-DB
            var data = _db.VitalMeasurements
                .AsNoTracking()
                .Where(x => x.ParticipantId == pid.Value)
                .OrderByDescending(x => x.MeasuredAt)
                .ToList();

            return View(data);
        }



        // ✅ רק WiFi דמו (אין ידני)
        [HttpPost]
        public IActionResult PullDeviceMeasurements(string? scenario)
        {
            if (Level != "B" && Level != "C")
                return Json(new { ok = false, message = "Service available only for Level B/C" });

            var pid = GetParticipantId();
            if (!pid.HasValue)
                return Json(new { ok = false, message = "Not logged in" });

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

            // Level B: חד פעמי בלבד - לא שומרים DB
            if (Level == "B")
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

            // Level C: שמירה ל-DB
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
                return Blocked("פרטי איש קשר לחירום", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var row = _db.EmergencyContacts.FirstOrDefault(x => x.ParticipantId == pid.Value);

            var vm = new EmergencyContactViewModel
            {
                FullName = row?.FullName ?? "",
                PhoneNumber = row?.PhoneNumber ?? "",
                Relationship = row?.Relationship ?? "",
                Message = row?.DefaultMessage ?? ""
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult EmergencyContact(EmergencyContactViewModel model)
        {
            if (Level != "B" && Level != "C")
                return Blocked("פרטי איש קשר לחירום", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            if (!ModelState.IsValid)
                return View(model);

            var row = _db.EmergencyContacts.FirstOrDefault(x => x.ParticipantId == pid.Value);

            if (row == null)
            {
                row = new EmergencyContactEntity
                {
                    ParticipantId = pid.Value
                };
                _db.EmergencyContacts.Add(row);
            }

            row.FullName = (model.FullName ?? "").Trim();
            row.PhoneNumber = (model.PhoneNumber ?? "").Trim();
            row.Relationship = (model.Relationship ?? "").Trim();
            row.DefaultMessage = (model.Message ?? "").Trim();
            row.UpdatedAtUtc = DateTime.UtcNow;

            _db.SaveChanges();

            TempData["SavedOk"] = "נשמר בהצלחה.";
            return RedirectToAction(nameof(EmergencyContact));
        }

        // ------------------------------------------------------------
        // B/C – קשר טקסטואלי בחירום (DB)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult EmergencyText()
        {
            if (Level != "B" && Level != "C")
                return Blocked("יצירת קשר טקסטואלי במקרה חירום", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            var contact = _db.EmergencyContacts.FirstOrDefault(x => x.ParticipantId == pid.Value);
            var draft = _db.EmergencyTextDrafts.FirstOrDefault(x => x.ParticipantId == pid.Value);

            var vm = new EmergencyTextViewModel
            {
                FullName = contact?.FullName ?? "",
                PhoneNumber = contact?.PhoneNumber ?? "",
                Address = draft?.Address ?? "",
                MessageText = draft?.MessageText ?? ""
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult EmergencyText(EmergencyTextViewModel model)
        {
            if (Level != "B" && Level != "C")
                return Blocked("יצירת קשר טקסטואלי במקרה חירום", "B");

            var pid = GetParticipantId();
            if (!pid.HasValue) return RedirectToAction("Login", "Experiment");

            if (!ModelState.IsValid)
                return View(model);

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
            return RedirectToAction(nameof(EmergencyText));
        }

        public class TeleVisitScheduleDto
        {
            public string? VisitType { get; set; }      // "video" / "phone"
            public string? PreferredDate { get; set; }  // yyyy-MM-dd
            public string? PreferredTime { get; set; }  // HH:mm
            public string? Reason { get; set; }
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

            // נשמור כ"תור טלפוני/וידיאו" דרך AppointmentRequestEntity
            // TreatmentArea -> "רופא" (דמו)
            // Notes -> נשמור visitType כדי שה-View ידע אם זה וידיאו או טלפוני
            var entity = new AppointmentRequestEntity
            {
                ParticipantId = pid.Value,
                TreatmentArea = "רופא (TeleVisit)",
                Reason = reason,
                PreferredDate = date,
                PreferredTime = time,
                Notes = visitType, // חשוב! ה-view בודק את זה
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

            // תורים עתידיים בלבד (כמו שרצית)
            // שימי לב: PreferredDate/PreferredTime הם string, אז נשתמש בסינון פשוט לפי תאריך במחרוזת
            // (אם את שומרת yyyy-MM-dd זה עובד טוב)
            var today = DateTime.Now.Date;

            var list = _db.AppointmentRequests
                .Where(x => x.ParticipantId == pid.Value)
                .AsNoTracking()
                .ToList();

            // מסננים "עתידיים" בצורה בטוחה יחסית לפי parsing
            var upcoming = new List<AppointmentRequestEntity>();
            foreach (var a in list)
            {
                // מצפים לפורמט yyyy-MM-dd ו-HH:mm
                DateTime dt;
                var dateStr = (a.PreferredDate ?? "").Trim();
                var timeStr = (a.PreferredTime ?? "").Trim();

                if (DateTime.TryParse($"{dateStr} {timeStr}", out dt))
                {
                    if (dt >= DateTime.Now.AddMinutes(-5)) // נותן טולרנס קטן
                        upcoming.Add(a);
                }
                else
                {
                    // אם לא הצלחנו לפרסר – עדיין נציג כדי שלא "ייעלם"
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

            // שעה מקומית (ישראל)
            var nowLocal = DateTime.Now;
            var hhmm = nowLocal.ToString("HH:mm");

            // כדי למנוע "קפיצה" כפולה באותה דקה - נשמור ב-Session מתי כבר שלחנו
            // key לדוגמה: 2026-01-25 15:00
            var minuteKey = nowLocal.ToString("yyyy-MM-dd HH:mm");
            var lastKey = HttpContext.Session.GetString("LastReminderMinuteKey");

            if (lastKey == minuteKey)
            {
                return Json(new { ok = true, now = hhmm, due = Array.Empty<object>() });
            }

            // מביאים תזכורות שמוגדרות לשעה הזו
            // Time אצלך נשמר "HH:mm" - בדיוק כמו hhmm
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

            // אם יש תזכורות - נעדכן session כדי לא להקפיץ שוב באותה דקה
            if (due.Any())
            {
                HttpContext.Session.SetString("LastReminderMinuteKey", minuteKey);
            }

            return Json(new { ok = true, now = hhmm, due });
        }
    }
}
