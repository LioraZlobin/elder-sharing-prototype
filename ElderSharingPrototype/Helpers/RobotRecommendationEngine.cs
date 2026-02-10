using System;
using System.Collections.Generic;
using System.Linq;

namespace ElderSharingPrototype.Helpers
{
    public class RobotRecommendation
    {
        public string Text { get; set; } = "";
        public string TargetUrl { get; set; } = "";
        public string ServiceKey { get; set; } = "";

        // שם קובץ mp3 מתוך wwwroot/audio/robot/
        public string AudioFile { get; set; } = "";

        // האם זו הקלטה/ניסוח של שאלה
        public bool IsQuestion { get; set; } = false;
    }

    public static class RobotRecommendationEngine
    {
        // =========================
        // רשימות בסיס לפי רמה
        // =========================
        private static readonly Dictionary<string, List<RobotRecommendation>> BaseByLevel =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // רמה A — פרטיות גבוהה
                ["A"] = new List<RobotRecommendation>
                {
                    new()
                    {
                        ServiceKey = "Vitals",
                        Text = "שלום. אני הרובוט שלך. ברמה אחת אנחנו שומרים על פרטיות גבוהה. אני ממליצה להתחיל בתזכורת למדדים. זה קצר, פשוט ובטוח.",
                        TargetUrl = "/Health/VitalsReminders",
                        AudioFile = "rec_A_1.mp3",
                        IsQuestion = false
                    },
                    new()
                    {
                        ServiceKey = "MedsGeneral",
                        Text = "היי. איך נוח לך היום? אם תרצי, אפשר להגדיר תזכורת כללית לנטילת תרופות. בלי לציין שמות.",
                        TargetUrl = "/Health/MedicationReminders",
                        AudioFile = "rec_A_2.mp3",
                        IsQuestion = false
                    },
                    new()
                    {
                        ServiceKey = "Checkup",
                        Text = "אני כאן איתך. כדאי לבדוק עכשיו את ההמלצות לביקור רופא ובדיקות שגרתיות. רק מידע כללי, ללא פרטים אישיים.",
                        TargetUrl = "/Health/CheckupRecommendations",
                        AudioFile = "rec_A_3.mp3",
                        IsQuestion = false
                    },
                    new()
                    {
                        ServiceKey = "VitalsQuestion",
                        Text = "תרצי שאעזור לך להגדיר תזכורת למדדים עכשיו?",
                        TargetUrl = "/Health/VitalsReminders",
                        AudioFile = "rec_A_4_question.mp3",
                        IsQuestion = true
                    }
                },

                // רמה B — שיתוף בינוני
                ["B"] = new List<RobotRecommendation>
                {
                    new()
                    {
                        ServiceKey = "MedList",
                        Text = "שלום. תודה שבחרת שיתוף בינוני. עכשיו אני יכולה להציע שירותים מותאמים יותר. אני ממליצה להתחיל בניהול רשימת תרופות קבועות.",
                        TargetUrl = "/Health/MedicationList",
                        AudioFile = "rec_B_1.mp3",
                        IsQuestion = false
                    },
                    new()
                    {
                        ServiceKey = "MedsByName",
                        Text = "כדי לעזור לך בצורה מדויקת, אפשר ליצור תזכורת לתרופה לפי שם ושעה. זה מקל על סדר יום ונותן שקט.",
                        TargetUrl = "/Health/MedicationReminders",
                        AudioFile = "rec_B_2.mp3",
                        IsQuestion = false
                    },
                    new()
                    {
                        ServiceKey = "EmergencyText",
                        Text = "אם יש משהו שמטריד אותך, אפשר לשלוח הודעת טקסט לאיש קשר לחירום. זו פעולה מהירה ובטוחה.",
                        TargetUrl = "/Health/EmergencyText",
                        AudioFile = "rec_B_3.mp3",
                        IsQuestion = false
                    },
                    new()
                    {
                        ServiceKey = "AppointmentQuestion",
                        Text = "תרצי לקבוע בקשה לתור לרופא? אני יכולה להכין את הבקשה לפי תחום טיפול.",
                        TargetUrl = "/Health/AppointmentRequest",
                        AudioFile = "rec_B_4_question.mp3",
                        IsQuestion = true
                    }
                },

                // רמה C — שיתוף גבוה
                ["C"] = new List<RobotRecommendation>
                {
                    new()
                    {
                        ServiceKey = "Measurements",
                        Text = "שלום. ברמה שלוש יש לי יכולת לעזור יותר. אני ממליצה להזין מדידות לחץ דם או סוכר. כך אפשר לעקוב בצורה מסודרת.",
                        TargetUrl = "/Health/Measurements",
                        AudioFile = "rec_C_1.mp3",
                        IsQuestion = false
                    },
                    new()
                    {
                        ServiceKey = "TeleVisit",
                        Text = "אם תרצי, אפשר לפתוח תור טלפוני או תור וידיאו. רק אם נוח לך—ואפשר לעצור בכל רגע.",
                        TargetUrl = "/Health/TeleVisit",
                        AudioFile = "rec_C_2.mp3",
                        IsQuestion = false
                    },
                    new()
                    {
                        ServiceKey = "EmergencyVideo",
                        Text = "במצב חירום, אפשר ליצור קשר חזותי עם איש קשר. אני אשאר איתך על המסך ואכוון צעד-אחר-צעד.",
                        TargetUrl = "/Health/EmergencyVideo",
                        AudioFile = "rec_C_3.mp3",
                        IsQuestion = false
                    },
                    new()
                    {
                        ServiceKey = "TeleVisitQuestion",
                        Text = "תרצי שאפעיל עכשיו תור וידיאו, או שנשמור את זה למועד אחר?",
                        TargetUrl = "/Health/TeleVisit",
                        AudioFile = "rec_C_4_question.mp3",
                        IsQuestion = true
                    }
                }
            };

        // =========================
        // ירושה: A ⊂ B ⊂ C
        // =========================
        private static IEnumerable<string> GetInheritanceChain(string level)
        {
            level = (level ?? "A").Trim().ToUpperInvariant();
            return level switch
            {
                "A" => new[] { "A" },
                "B" => new[] { "A", "B" },
                "C" => new[] { "A", "B", "C" },
                _ => new[] { "A" }
            };
        }

        private static List<RobotRecommendation> BuildPool(string level)
        {
            var chain = GetInheritanceChain(level);

            // מאחדים את כל ההמלצות מהשרשרת (A/B/C)
            var pool = new List<RobotRecommendation>();

            foreach (var lvl in chain)
            {
                if (BaseByLevel.TryGetValue(lvl, out var list) && list != null && list.Count > 0)
                    pool.AddRange(list);
            }

            // אם במקרה יש ServiceKey כפולים (אפשרי בעתיד) – נעדיף את הגבוה יותר בשרשרת
            // כלומר אם מישהו יוסיף בעתיד אותו key ל-B ול-A, ניקח את האחרון שנוסף (הגבוה יותר)
            pool = pool
                .GroupBy(r => (r.ServiceKey ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Last())
                .ToList();

            return pool;
        }

        public static RobotRecommendation GetRandom(string level, string? lastKey = null)
        {
            var pool = BuildPool(level);
            if (pool.Count == 0)
                return new RobotRecommendation();

            if (pool.Count == 1 || string.IsNullOrWhiteSpace(lastKey))
                return pool[Random.Shared.Next(pool.Count)];

            RobotRecommendation pick;
            int guard = 0;

            do
            {
                pick = pool[Random.Shared.Next(pool.Count)];
                guard++;
            }
            while (pick.ServiceKey.Equals(lastKey, StringComparison.OrdinalIgnoreCase) && guard < 10);

            return pick;
        }
    }
}
