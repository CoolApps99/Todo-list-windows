using System;

namespace TodoListApp
{
    public class Config
    {
        public bool DarkTheme { get; set; } = false;
        public bool ConfirmDelete { get; set; } = true;

        // Recurring Reminders
        public bool EnableWaterReminder { get; set; } = false;
        public int WaterReminderIntervalMinutes { get; set; } = 120; // Default 2 hours
        public DateTime? LastWaterReminderTime { get; set; } = null;

        public bool EnableStandUpReminder { get; set; } = false;
        public int StandUpReminderIntervalMinutes { get; set; } = 60; // Default 1 hour
        public DateTime? LastStandUpReminderTime { get; set; } = null;

        // Window position/size (optional enhancement)
        public int WindowLeft { get; set; } = -1; // Use -1 to indicate default position
        public int WindowTop { get; set; } = -1;
        public int WindowWidth { get; set; } = 550;
        public int WindowHeight { get; set; } = 600;
    }
}