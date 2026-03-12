using System.Collections.Generic;

namespace ElderSharingPrototype.Models.Health
{
    public class MedicationCenterViewModel
    {
        public string Level { get; set; } = "A";

        public List<string> MedicationList { get; set; } = new();
        public List<string> MedicationCatalog { get; set; } = new();

        public List<ReminderItem> Reminders { get; set; } = new();
    }
}