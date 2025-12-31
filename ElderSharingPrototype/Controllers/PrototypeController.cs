using ElderSharingPrototype.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElderSharingPrototype.Controllers
{
    public class PrototypeController : Controller
    {
        private static List<string> GetDiseaseCatalog()
        {
            return new List<string>
    {
        // --- א ---
        "אסתמה", "אלרגיות", "אי ספיקת לב", "אי ספיקת כליות", "אוסטאופורוזיס", "אנמיה", "אפילפסיה",
        "ארתריטיס (דלקת מפרקים)", "אי-סדירות בקצב הלב",

        // --- ב ---
        "ברונכיטיס כרונית", "בעיות בלוטת התריס", "בעיות גב כרוניות",

        // --- ג ---
        "גאוט", "גלאוקומה", "גסטריטיס", "גרד כרוני",

        // --- ד ---
        "דמנציה", "דיכאון", "דלקת מפרקים שגרונית", "דלקת ריאות חוזרת", "דום נשימה בשינה", "דלקת כרונית במעי (IBD)",

        // --- ה ---
        "היפרכולסטרולמיה (כולסטרול גבוה)", "היפרלחץ דם", "היפוגליקמיה (נפילות סוכר)",

        // --- ו ---
        "ורטיגו (סחרחורות)",

        // --- ז ---
        "זיהומים חוזרים בדרכי השתן", "זיהום כרוני",

        // --- ח ---
        "חרדה", "חסימת עורקים", "חולשת שרירים כרונית", "חומציות יתר (רפלוקס)",

        // --- ט ---
        "טחורים", "טרשת נפוצה", "טרשת עורקים", "טינטון",

        // --- י ---
        "יתר לחץ דם", "יתר פעילות בלוטת התריס", "תת פעילות בלוטת התריס",

        // --- כ ---
        "כיב קיבה", "כאבי גב כרוניים", "כאבי מפרקים",

        // --- ל ---
        "לחץ דם גבוה", "לופוס", "ליקוי שמיעה", "ליקוי ראייה",

        // --- מ ---
        "מחלת לב איסכמית", "מחלת ריאות חסימתית כרונית (COPD)", "מחלת כליות כרונית", "מחלת כבד שומני",
        "מיגרנה", "מחלות עור",

        // --- נ ---
        "נשירת שיער", "נפילות חוזרות", "נוירופתיה (פגיעה עצבית)",

        // --- ס ---
        "סוכרת סוג 1", "סוכרת סוג 2", "סינוסיטיס כרונית", "סרטן", "סכיזופרניה",

        // --- ע ---
        "עצירות כרונית", "עייפות כרונית", "עיוורון חלקי",

        // --- פ ---
        "פרקינסון", "פסוריאזיס", "פיברומיאלגיה",

        // --- צ ---
        "צליאק", "צפדינה", "צנתור/סטנט בעבר",

        // --- ק ---
        "קרישיות יתר", "קטרקט", "קוליטיס", "קוצר נשימה כרוני",

        // --- ר ---
        "רפלוקס (GERD)", "רעידות", "רגישות ללקטוז",

        // --- ש ---
        "שבץ מוחי בעבר", "שומנים גבוהים", "שברים חוזרים", "שינה לא סדירה",

        // --- ת ---
        "תסמונת המעי הרגיז (IBS)", "תסמונת מטבולית", "תעוקת חזה"
    };
        }

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
        // מסך 1 – הסבר
        public IActionResult Intro()
        {
            return View();
        }

        // מסך 2 – בחירת רמה
        [HttpGet]
        public IActionResult Choose()
        {
            return View();
        }

        // שמירת רמה והמשך למסך הגדרות לפי רמה
        [HttpPost]
        public IActionResult Choose(string level)
        {
            HttpContext.Session.SetString("SharingLevel", level ?? "A");
            return RedirectToAction("RegisterMini");
        }

        // מסך 3 – הגדרות לפי רמה (3A/3B/3C באותו View)
        [HttpGet]
        public IActionResult RegisterMini()
        {
            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";
            ViewBag.Level = level;

            // ✅ שולחים ל-View את קטלוג התרופות
            ViewBag.MedCatalog = GetMedicationCatalog();
            ViewBag.DiseaseCatalog = GetDiseaseCatalog();


            // ✅ מחלות ותרופות - שולפים בחירות קודמות אם קיימות
            var savedMeds = HttpContext.Session.GetString("FixedMedications") ?? "";
            var selectedMeds = savedMeds.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim()).ToList();

            var savedDiseases = HttpContext.Session.GetString("FixedDiseases") ?? "";
            var selectedDiseases = savedDiseases.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                                .Select(x => x.Trim()).ToList();

            var vm = new MiniRegisterViewModel
            {
                FirstName = HttpContext.Session.GetString("FirstName"),
                Language = HttpContext.Session.GetString("Language"),
                Age = int.TryParse(HttpContext.Session.GetString("Age"), out var a) ? a : null,
                FixedMedications = savedMeds,
                SelectedMedications = selectedMeds,
                FixedDiseases = savedDiseases,
                SelectedDiseases = selectedDiseases,
                IdNumber = HttpContext.Session.GetString("IdNumber")
            };

            return View(vm);
        }


        // הרשמה/הגדרות – POST
        [HttpPost]
        public IActionResult RegisterMini(MiniRegisterViewModel model)
        {
            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";

            // ✅ ולידציה לפי דרישות חדשות
            if (string.IsNullOrWhiteSpace(model.FirstName))
                ModelState.AddModelError(nameof(model.FirstName), "נא להזין שם.");

            if (!model.Age.HasValue || model.Age < 60 || model.Age > 120)
                ModelState.AddModelError(nameof(model.Age), "נא להזין גיל תקין (60–120).");

            if (string.IsNullOrWhiteSpace(model.Language))
                ModelState.AddModelError(nameof(model.Language), "נא לבחור שפה.");

            if ((level == "B" || level == "C") && (model.SelectedMedications == null || !model.SelectedMedications.Any()))
                ModelState.AddModelError(nameof(model.SelectedMedications), "נא לבחור לפחות תרופה אחת מהרשימה.");

            if ((level == "B" || level == "C") && (model.SelectedDiseases == null || !model.SelectedDiseases.Any()))
                ModelState.AddModelError(nameof(model.SelectedDiseases), "נא לבחור לפחות מחלה אחת מהרשימה.");

            if (level == "C")
            {
                var id = (model.IdNumber ?? "").Trim();
                if (id.Length != 9 || !id.All(char.IsDigit))
                    ModelState.AddModelError(nameof(model.IdNumber), "נא להזין תעודת זהות תקינה (9 ספרות).");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Level = level;
                ViewBag.MedCatalog = GetMedicationCatalog();
                ViewBag.DiseaseCatalog = GetDiseaseCatalog();
                return View(model);
            }



            // ✅ שמירה ב-Session
            HttpContext.Session.SetString("FirstName", model.FirstName!.Trim());
            HttpContext.Session.SetString("Language", model.Language!.Trim());
            HttpContext.Session.SetString("Age", model.Age!.Value.ToString());

            if (level == "B" || level == "C")
            {
                // תרופות
                var meds = model.SelectedMedications ?? new List<string>();
                HttpContext.Session.SetString(
                    "FixedMedications",
                    string.Join(", ", meds)
                );

                // מחלות
                var diseases = model.SelectedDiseases ?? new List<string>();
                HttpContext.Session.SetString(
                    "FixedDiseases",
                    string.Join(", ", diseases)
                );
            }
            else
            {
                HttpContext.Session.Remove("FixedMedications");
                HttpContext.Session.Remove("FixedDiseases");
            }


            if (level == "C")
                HttpContext.Session.SetString("IdNumber", (model.IdNumber ?? "").Trim());
            else
                HttpContext.Session.Remove("IdNumber");

            HttpContext.Session.SetString("IsLoggedIn", "true");
            return RedirectToAction("Services");
        }

        // מסך שירותים (בריאות בלבד)
        public IActionResult Services()
        {
            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";
            ViewBag.Level = level;

            ViewBag.FirstName = HttpContext.Session.GetString("FirstName") ?? "";
            ViewBag.Language = HttpContext.Session.GetString("Language") ?? "";
            ViewBag.FixedMedications = HttpContext.Session.GetString("FixedMedications") ?? "";
            ViewBag.IdNumber = HttpContext.Session.GetString("IdNumber") ?? "";

            return View();
        }

        // ✅ שדרוג רמה – צעד אחד קדימה (A→B, B→C)
        [HttpPost]
        public IActionResult UpgradeLevel()
        {
            var current = HttpContext.Session.GetString("SharingLevel") ?? "A";
            var next = current == "A" ? "B" : current == "B" ? "C" : "C";

            HttpContext.Session.SetString("SharingLevel", next);

            // אחרי שדרוג עוברים למסך ההגדרות להשלים פרטים
            return RedirectToAction("RegisterMini");
        }

        // ✅ הורדת רמת שיתוף (לפי דרישה: אפשר לצמצם בכל רגע)
        [HttpPost]
        public IActionResult DowngradeLevel(string targetLevel)
        {
            // מאפשרים רק A/B/C
            var t = (targetLevel ?? "A").ToUpperInvariant();
            if (t != "A" && t != "B" && t != "C") t = "A";

            HttpContext.Session.SetString("SharingLevel", t);

            // ניקוי מידע שלא רלוונטי לרמה נמוכה יותר
            if (t == "A")
            {
                HttpContext.Session.Remove("FixedMedications");
                HttpContext.Session.Remove("FixedDiseases");
                HttpContext.Session.Remove("IdNumber");
            }
            else if (t == "B")
            {
                HttpContext.Session.Remove("IdNumber");
            }

            return RedirectToAction("RegisterMini");
        }

        // ✅ מחיקת מידע – דרישה מפורשת
        [HttpPost]
        public IActionResult DeleteMyData()
        {
            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";

            HttpContext.Session.Remove("FirstName");
            HttpContext.Session.Remove("Language");
            HttpContext.Session.Remove("FixedMedications");
            HttpContext.Session.Remove("IdNumber");
            HttpContext.Session.Remove("Age");
            HttpContext.Session.Remove("FixedDiseases");
            HttpContext.Session.SetString("SharingLevel", level);
            HttpContext.Session.SetString("IsLoggedIn", "false");

            return RedirectToAction("Intro");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Intro");
        }
    }
}
