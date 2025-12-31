using ElderSharingPrototype.Helpers;
using ElderSharingPrototype.Models.Health;
using Microsoft.AspNetCore.Mvc;

namespace ElderSharingPrototype.Controllers
{
    public class HealthController : Controller
    {
        private string Level => HttpContext.Session.GetString("SharingLevel") ?? "A";

        private IActionResult Blocked(string serviceName, string targetLevel)
        {
            TempData["BlockedService"] = serviceName;
            TempData["TargetLevel"] = targetLevel;
            return RedirectToAction("Services", "Prototype");
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------
        private List<string> GetUserMedicationList()
        {
            // "אקמול, אספירין, ..."
            var fixedMeds = HttpContext.Session.GetString("FixedMedications") ?? "";
            if (string.IsNullOrWhiteSpace(fixedMeds))
                return new List<string>();

            return fixedMeds
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // ✅ קטלוג תרופות (כמו בהרשמה) כדי להציג Select2 ב-MedicationList
        private static List<string> GetMedicationCatalog()
        {
            return new List<string>
            {
                // --- א ---
                "אקמול", "אדויל", "אספירין", "אומפרדקס", "אומפרזול", "אנטיהיסטמין",

                // --- ב ---
                "ברופן", "בטאקורטן", "ביסופרולול", "ברזל", "בודיקורט",

                // --- ג ---
                "גלוקופאג׳", "גסטרו", "ג׳לוסיל", "גאבאפנטין", "גינרה",

                // --- ד ---
                "דקסמול", "דקסמול קולד", "דופסטון", "דיאמיקרון", "דוקסילין",

                // --- ה ---
                "הידרוכלורותיאזיד", "הידרוקסיזין", "הפארין",

                // --- ו ---
                "ויטמין D", "ויטמין B12", "וולטרן", "ורמוקס",

                // --- ז ---
                "זודורם", "זירקט", "זינאט",

                // --- ח ---
                "חומצה פולית",

                // --- ט ---
                "טאמסולין", "טמסולוסין", "טראמדקס", "טוביאז",

                // --- י ---
                "יאז", "יוויטל",

                // --- כ ---
                "כדורי ברזל", "כדורי סידן",

                // --- ל ---
                "לוסק", "לוריוון", "לורטאדין", "לבותרוקסין", "לנטוס",

                // --- מ ---
                "מטפורמין", "מוקסיפן", "מגנזיום", "מיקרופירין", "מונולונג",

                // --- נ ---
                "נורופן", "נקסיום", "נורמלקס", "נורמיטן",

                // --- ס ---
                "סטטינים", "סימוביל", "סימבסטטין", "סולפיריד", "סולפיד",

                // --- פ ---
                "פראמין", "פרדניזון", "פרדניזולון", "פלביקס", "פנרגן",

                // --- צ ---
                "ציפרודקס", "ציפרופלוקסצין", "ציפרודין", "ציפרלקס", "צנטרום",

                // --- ק ---
                "קונקור", "קלונקס", "קלקסן", "קווינפריל", "קולכיצין",

                // --- ר ---
                "ריטלין", "רוקסט", "רניטידין", "רמרון",

                // --- ת ---
                "תירוקסין", "תמיפלו"
            };
        }

        // ------------------------------------------------------------
        // ניהול רשימת תרופות קבועות (B/C)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult MedicationList()
        {
            if (Level != "B" && Level != "C")
                return Blocked("ניהול רשימת תרופות קבועות", "B");

            ViewBag.Level = Level;

            // ✅ שולחים את הקטלוג כדי שה-View יוכל להציג select2
            ViewBag.MedCatalog = GetMedicationCatalog();

            var meds = GetUserMedicationList();
            return View(meds);
        }

        [HttpPost]
        public IActionResult RemoveMedicationFromList(string name)
        {
            if (Level != "B" && Level != "C")
                return Blocked("ניהול רשימת תרופות קבועות", "B");

            var meds = GetUserMedicationList();
            var toRemove = (name ?? "").Trim();

            meds = meds
                .Where(m => !string.Equals(m, toRemove, StringComparison.OrdinalIgnoreCase))
                .ToList();

            HttpContext.Session.SetString("FixedMedications", string.Join(", ", meds));
            return RedirectToAction(nameof(MedicationList));
        }

        [HttpPost]
        public IActionResult AddMedicationToList(string name)
        {
            if (Level != "B" && Level != "C")
                return Blocked("ניהול רשימת תרופות קבועות", "B");

            var meds = GetUserMedicationList();
            var newName = (name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                TempData["MedListError"] = "נא לבחור תרופה מהרשימה להוספה.";
                return RedirectToAction(nameof(MedicationList));
            }

            // ✅ מאפשרים הוספה רק אם קיימת בקטלוג (כמו בהרשמה)
            var catalog = GetMedicationCatalog();
            var inCatalog = catalog.Any(m => string.Equals(m, newName, StringComparison.OrdinalIgnoreCase));
            if (!inCatalog)
            {
                TempData["MedListError"] = "התרופה שנבחרה אינה קיימת בקטלוג. נא לבחור מהרשימה.";
                return RedirectToAction(nameof(MedicationList));
            }

            // ✅ אם כבר קיימת — הודעה ולא מוסיפים כפול
            if (meds.Any(m => string.Equals(m, newName, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["MedListError"] = "התרופה כבר קיימת ברשימה שלך.";
                return RedirectToAction(nameof(MedicationList));
            }

            meds.Add(newName);
            meds = meds.OrderBy(x => x).ToList();

            HttpContext.Session.SetString("FixedMedications", string.Join(", ", meds));
            return RedirectToAction(nameof(MedicationList));
        }

        // ------------------------------------------------------------
        // תזכורות תרופות
        // A: תזכורת כללית
        // B/C: תזכורת עם שם תרופה מתוך הרשימה האישית
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult MedicationReminders()
        {
            var level = Level; // A/B/C
            ViewBag.Level = level;

            ViewBag.UserMeds = GetUserMedicationList();

            var list = HttpContext.Session.GetObject<List<ReminderItem>>("MedReminders") ?? new List<ReminderItem>();
            return View(list.OrderByDescending(x => x.CreatedAt).ToList());
        }

        [HttpPost]
        public IActionResult AddMedicationReminder(string? medicationName, string time, string? notes)
        {
            var level = Level; // A/B/C
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
                    {
                        ModelState.AddModelError("medicationName", "נא לבחור תרופה מהרשימה שלך.");
                    }
                    else
                    {
                        var chosen = medicationName.Trim();
                        var exists = userMeds.Any(m => string.Equals(m, chosen, StringComparison.OrdinalIgnoreCase));
                        if (!exists)
                            ModelState.AddModelError("medicationName", "התרופה שנבחרה אינה נמצאת ברשימה שלך. נא לבחור תרופה מהרשימה.");
                    }
                }
            }

            var list = HttpContext.Session.GetObject<List<ReminderItem>>("MedReminders") ?? new List<ReminderItem>();

            if (!ModelState.IsValid)
                return View("MedicationReminders", list.OrderByDescending(x => x.CreatedAt).ToList());

            ReminderItem item;

            if (level == "B" || level == "C")
            {
                var chosen = medicationName!.Trim();

                item = new ReminderItem
                {
                    Type = "Medication",
                    MedicationName = chosen,
                    Title = $"תזכורת לנטילת: {chosen}",
                    Time = time.Trim(),
                    Notes = (notes ?? "").Trim()
                };
            }
            else
            {
                item = new ReminderItem
                {
                    Type = "Medication",
                    MedicationName = null,
                    Title = "תזכורת כללית ללקיחת תרופות",
                    Time = time.Trim(),
                    Notes = (notes ?? "").Trim()
                };
            }

            list.Add(item);
            HttpContext.Session.SetObject("MedReminders", list);

            return RedirectToAction(nameof(MedicationReminders));
        }

        [HttpPost]
        public IActionResult DeleteMedicationReminder(Guid id)
        {
            var list = HttpContext.Session.GetObject<List<ReminderItem>>("MedReminders") ?? new List<ReminderItem>();
            list = list.Where(x => x.Id != id).ToList();
            HttpContext.Session.SetObject("MedReminders", list);
            return RedirectToAction(nameof(MedicationReminders));
        }

        // ------------------------------------------------------------
        // תזכורות מדדים (A/B/C)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult VitalsReminders()
        {
            var list = HttpContext.Session.GetObject<List<ReminderItem>>("VitalsReminders") ?? new List<ReminderItem>();
            return View(list.OrderByDescending(x => x.CreatedAt).ToList());
        }

        [HttpPost]
        public IActionResult AddVitalsReminder(string vitalType, string time)
        {
            if (string.IsNullOrWhiteSpace(vitalType))
                ModelState.AddModelError("vitalType", "נא לבחור סוג מדד.");
            if (string.IsNullOrWhiteSpace(time))
                ModelState.AddModelError("time", "נא לבחור שעה.");

            var list = HttpContext.Session.GetObject<List<ReminderItem>>("VitalsReminders") ?? new List<ReminderItem>();

            if (!ModelState.IsValid)
                return View("VitalsReminders", list);

            list.Add(new ReminderItem
            {
                Type = "Vitals",
                Title = vitalType.Trim(),
                Time = time.Trim(),
                Notes = ""
            });

            HttpContext.Session.SetObject("VitalsReminders", list);
            return RedirectToAction(nameof(VitalsReminders));
        }

        [HttpPost]
        public IActionResult DeleteVitalsReminder(Guid id)
        {
            var list = HttpContext.Session.GetObject<List<ReminderItem>>("VitalsReminders") ?? new List<ReminderItem>();
            list = list.Where(x => x.Id != id).ToList();
            HttpContext.Session.SetObject("VitalsReminders", list);
            return RedirectToAction(nameof(VitalsReminders));
        }

        // ------------------------------------------------------------
        // הזנת מדידות (C)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult Measurements()
        {
            if (Level != "C")
                return Blocked("הזנת מדידות", "C");

            var list = HttpContext.Session.GetObject<List<VitalMeasurement>>("Measurements") ?? new List<VitalMeasurement>();
            return View(list.OrderByDescending(x => x.MeasuredAt).ToList());
        }

        [HttpPost]
        public IActionResult AddBloodPressure(int? systolic, int? diastolic, string? notes)
        {
            if (Level != "C")
                return Blocked("הזנת מדידות", "C");

            if (!systolic.HasValue || systolic < 50 || systolic > 250)
                ModelState.AddModelError("systolic", "נא להזין ערך עליון תקין.");
            if (!diastolic.HasValue || diastolic < 30 || diastolic > 150)
                ModelState.AddModelError("diastolic", "נא להזין ערך תחתון תקין.");

            var list = HttpContext.Session.GetObject<List<VitalMeasurement>>("Measurements") ?? new List<VitalMeasurement>();
            if (!ModelState.IsValid)
                return View("Measurements", list.OrderByDescending(x => x.MeasuredAt).ToList());

            list.Add(new VitalMeasurement
            {
                Kind = "BloodPressure",
                Systolic = systolic,
                Diastolic = diastolic,
                Notes = (notes ?? "").Trim()
            });

            HttpContext.Session.SetObject("Measurements", list);
            return RedirectToAction(nameof(Measurements));
        }

        [HttpPost]
        public IActionResult AddSugar(int? sugarMgDl, string? notes)
        {
            if (Level != "C")
                return Blocked("הזנת מדידות", "C");

            if (!sugarMgDl.HasValue || sugarMgDl < 30 || sugarMgDl > 600)
                ModelState.AddModelError("sugarMgDl", "נא להזין ערך סוכר תקין.");

            var list = HttpContext.Session.GetObject<List<VitalMeasurement>>("Measurements") ?? new List<VitalMeasurement>();
            if (!ModelState.IsValid)
                return View("Measurements", list.OrderByDescending(x => x.MeasuredAt).ToList());

            list.Add(new VitalMeasurement
            {
                Kind = "Sugar",
                SugarMgDl = sugarMgDl,
                Notes = (notes ?? "").Trim()
            });

            HttpContext.Session.SetObject("Measurements", list);
            return RedirectToAction(nameof(Measurements));
        }

        // ------------------------------------------------------------
        // זימון תור (C) - דמו
        // ------------------------------------------------------------
        // ------------------------------------------------------------
        // זימון תור (C) - דמו
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult AppointmentRequest()
        {
            if (Level != "C")
                return Blocked("זימון תור לרופא", "C");

            var list = HttpContext.Session.GetObject<List<AppointmentRequest>>("Appointments") ?? new List<AppointmentRequest>();
            ViewBag.Requests = list.OrderByDescending(x => x.CreatedAt).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult AddAppointment(string reason, string preferredDate, string preferredTime, string? notes)
        {
            if (Level != "C")
                return Blocked("זימון תור לרופא", "C");

            if (string.IsNullOrWhiteSpace(reason))
                ModelState.AddModelError("reason", "נא לציין סיבה.");
            if (string.IsNullOrWhiteSpace(preferredDate))
                ModelState.AddModelError("preferredDate", "נא לבחור תאריך.");
            if (string.IsNullOrWhiteSpace(preferredTime))
                ModelState.AddModelError("preferredTime", "נא לבחור שעה.");

            var list = HttpContext.Session.GetObject<List<AppointmentRequest>>("Appointments") ?? new List<AppointmentRequest>();

            if (!ModelState.IsValid)
            {
                ViewBag.Requests = list.OrderByDescending(x => x.CreatedAt).ToList();
                return View("AppointmentRequest");
            }

            list.Add(new AppointmentRequest
            {
                Reason = reason.Trim(),
                PreferredDate = preferredDate.Trim(),
                PreferredTime = preferredTime.Trim(),
                Notes = (notes ?? "").Trim()
            });

            HttpContext.Session.SetObject("Appointments", list);
            return RedirectToAction(nameof(AppointmentRequest));
        }

        [HttpPost]
        public IActionResult DeleteAppointment(Guid id)
        {
            if (Level != "C")
                return Blocked("זימון תור לרופא", "C");

            var list = HttpContext.Session.GetObject<List<AppointmentRequest>>("Appointments") ?? new List<AppointmentRequest>();
            list = list.Where(x => x.Id != id).ToList();

            HttpContext.Session.SetObject("Appointments", list);
            return RedirectToAction(nameof(AppointmentRequest));
        }

    }
}
