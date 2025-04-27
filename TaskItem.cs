using System;

namespace TodoListApp
{
    public class TaskItem
    {
        public string Text { get; set; } = string.Empty;
        public string Priority { get; set; } = "None";
        public bool IsChecked { get; set; } = false;
        public DateTime? ReminderTime { get; set; } // Nullable DateTime for reminders
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique ID for reliable updates/finding
    }
}