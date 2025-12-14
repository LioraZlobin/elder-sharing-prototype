using ElderSharingPrototype.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElderSharingPrototype.Controllers
{
    public class PrototypeController : Controller
    {
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

        // שמירת רמה והמשך להרשמה קצרה
        [HttpPost]
        public IActionResult Choose(string level)
        {
            HttpContext.Session.SetString("SharingLevel", level ?? "A");
            return RedirectToAction("RegisterMini");
        }

        // הרשמה קצרה – GET
        [HttpGet]
        public IActionResult RegisterMini()
        {
            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";
            ViewBag.Level = level;

            var vm = new MiniRegisterViewModel
            {
                // רמה A
                FirstName = HttpContext.Session.GetString("FirstName"),
                Area = HttpContext.Session.GetString("Area"),
                Age = int.TryParse(HttpContext.Session.GetString("Age"), out var a) ? a : null,

                // רמה B+
                Hmo = HttpContext.Session.GetString("Hmo"),
                Medications = HttpContext.Session.GetString("Medications"),

                // רמה C
                EmergencyContactName = HttpContext.Session.GetString("EmergencyContactName"),
                EmergencyContactPhone = HttpContext.Session.GetString("EmergencyContactPhone"),
                CreditCardLast4 = HttpContext.Session.GetString("CreditCardLast4"),
                CreditCardExpiry = HttpContext.Session.GetString("CreditCardExpiry")
            };

            return View(vm);
        }

        // הרשמה קצרה – POST
        [HttpPost]
        public IActionResult RegisterMini(MiniRegisterViewModel model)
        {
            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";

            // ✅ ולידציה לפי המקרא + אשראי ברמה C
            if (string.IsNullOrWhiteSpace(model.FirstName))
                ModelState.AddModelError(nameof(model.FirstName), "נא להזין שם פרטי.");

            if (!model.Age.HasValue || model.Age < 60 || model.Age > 120)
                ModelState.AddModelError(nameof(model.Age), "נא להזין גיל תקין (60–120).");

            if (string.IsNullOrWhiteSpace(model.Area))
                ModelState.AddModelError(nameof(model.Area), "נא לבחור אזור מגורים.");

            if ((level == "B" || level == "C") && string.IsNullOrWhiteSpace(model.Hmo))
                ModelState.AddModelError(nameof(model.Hmo), "נא לבחור קופת חולים.");

            if (level == "C")
            {
                if (string.IsNullOrWhiteSpace(model.EmergencyContactName))
                    ModelState.AddModelError(nameof(model.EmergencyContactName), "נא להזין שם איש קשר לחירום.");

                if (string.IsNullOrWhiteSpace(model.EmergencyContactPhone))
                    ModelState.AddModelError(nameof(model.EmergencyContactPhone), "נא להזין טלפון איש קשר לחירום.");

                // אשראי: רק 4 ספרות אחרונות + תוקף
                var last4 = (model.CreditCardLast4 ?? "").Trim();
                if (last4.Length != 4 || !last4.All(char.IsDigit))
                    ModelState.AddModelError(nameof(model.CreditCardLast4), "נא להזין 4 ספרות אחרונות (מספרים בלבד).");

                if (string.IsNullOrWhiteSpace(model.CreditCardExpiry))
                    ModelState.AddModelError(nameof(model.CreditCardExpiry), "נא להזין תוקף כרטיס (MM/YY).");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Level = level;
                return View(model);
            }

            // ✅ שמירה ב-Session
            HttpContext.Session.SetString("FirstName", model.FirstName!.Trim());
            HttpContext.Session.SetString("Age", model.Age!.Value.ToString());
            HttpContext.Session.SetString("Area", model.Area!.Trim());

            if (level == "B" || level == "C")
            {
                HttpContext.Session.SetString("Hmo", (model.Hmo ?? "").Trim());
                HttpContext.Session.SetString("Medications", (model.Medications ?? "").Trim());
            }
            else
            {
                // ניקוי אם ירדו לרמה A
                HttpContext.Session.Remove("Hmo");
                HttpContext.Session.Remove("Medications");
            }

            if (level == "C")
            {
                HttpContext.Session.SetString("EmergencyContactName", (model.EmergencyContactName ?? "").Trim());
                HttpContext.Session.SetString("EmergencyContactPhone", (model.EmergencyContactPhone ?? "").Trim());
                HttpContext.Session.SetString("CreditCardLast4", (model.CreditCardLast4 ?? "").Trim());
                HttpContext.Session.SetString("CreditCardExpiry", (model.CreditCardExpiry ?? "").Trim());
            }
            else
            {
                // ניקוי אם ירדו מרמה C
                HttpContext.Session.Remove("EmergencyContactName");
                HttpContext.Session.Remove("EmergencyContactPhone");
                HttpContext.Session.Remove("CreditCardLast4");
                HttpContext.Session.Remove("CreditCardExpiry");
            }

            HttpContext.Session.SetString("IsLoggedIn", "true");
            return RedirectToAction("Services");
        }

        // מסך 3 – שירותים
        public IActionResult Services()
        {
            var level = HttpContext.Session.GetString("SharingLevel") ?? "A";
            ViewBag.Level = level;

            // נציג כרטיס רק ברמה C ואם קיימות 4 ספרות אחרונות
            if (level == "C")
            {
                ViewBag.CreditCardLast4 = HttpContext.Session.GetString("CreditCardLast4") ?? "";
            }
            else
            {
                ViewBag.CreditCardLast4 = "";
            }

            return View();
        }

        // שדרוג רמה (ממודאל במסך השירותים)
        [HttpPost]
        public IActionResult UpgradeLevel(string targetLevel)
        {
            HttpContext.Session.SetString("SharingLevel", targetLevel ?? "B");

            // אחרי שדרוג – עוברים להרשמה כדי להשלים פרטים שנדרשים לרמה החדשה
            return RedirectToAction("RegisterMini");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Intro");
        }

    }
}
