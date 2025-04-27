using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using System.Globalization;


namespace TodoListApp
{
    public partial class MainForm : Form
    {
        // --- Controls ---
        private ListView lvTasks;
        private TextBox txtNewTask;
        private Button btnAdd, btnTheme, btnDeleteAll, btnOptions, btnInfo;
        private Panel headerPanel, inputPanel;
        private ImageList lvImageList;
        private ContextMenuStrip taskContextMenuStrip;
        private System.Windows.Forms.Timer reminderTimer;

        // --- Data & Config ---
        private readonly string todoFilePath;
        private readonly string configPath;
        private Config appConfig = new Config();
        private List<TaskItem> taskList = new List<TaskItem>();

        // --- Styling Colors ---
        private readonly Color priorityColorNone = Color.Gray;
        private readonly Color priorityColorLow = Color.MediumSeaGreen;
        private readonly Color priorityColorMedium = Color.Goldenrod;
        private readonly Color priorityColorHigh = Color.Tomato;
        private Color listBackColorEven, listBackColorOdd, listSelectionBorderColor, listForeColor, gridColor, headerColor1, headerColor2, formBackColor, inputPanelBackColor, textBoxBackColor;
        // <<< CHANGE: Added color for delete X >>>
        private Color deleteIconColor = Color.IndianRed;
        private Color deleteIconHoverColor = Color.Red; // Optional: for hover effect

        // --- System Tray ---
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        // --- State Flags ---
        private bool suppressNextItemCheck = false;
        private bool isEditing = false;
        private bool reallyClosing = false;
        private bool darkTheme = false;
        // <<< CHANGE: Track mouse hover for delete icon (optional) >>>
        private ListViewItem.ListViewSubItem? hoveredDeleteSubItem = null;


        // --- Constants ---
        // <<< CHANGE: Removed REMINDER_ICON constant, reminder is text now >>>
        private const int REMINDER_TIMER_INTERVAL_MS = 15 * 1000;


        public MainForm()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "SimpleTodoListApp");
            Directory.CreateDirectory(appFolder);
            todoFilePath = Path.Combine(appFolder, "todo.json");
            configPath = Path.Combine(appFolder, "config.json");

            LoadConfig();
            InitializeUI();
            InitializeTaskContextMenu();
            InitializeTray();
            InitializeReminderTimer();
            LoadTasks();
            PopulateListView();
            ApplyTheme();
        }

        // --- Initialization ---

        private void InitializeUI()
        {
            this.Text = "Simple To-Do List";
            this.Size = new Size(400, 400); // <<< CHANGE: Wider to accommodate new columns >>>
            this.MinimumSize = new Size(200, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);

            headerPanel = new Panel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(5) };
            headerPanel.Paint += HeaderPanel_Paint;

            // Header Buttons (Right-to-Left for Docking Order)
            btnTheme = CreateHeaderButton("?", "btnTheme", 38);
            btnTheme.Font = new Font("Segoe UI Symbol", 12F, FontStyle.Regular);
            btnTheme.Click += BtnTheme_Click;
            headerPanel.Controls.Add(btnTheme);

            btnOptions = CreateHeaderButton("⚙", "btnOptions", 38);
            btnOptions.Font = new Font("Segoe UI Symbol", 12F);
            btnOptions.Click += BtnOptions_Click;
            headerPanel.Controls.Add(btnOptions);

            btnDeleteAll = CreateHeaderButton("🗑", "btnDeleteAll", 38);
            btnDeleteAll.Font = new Font("Segoe UI Symbol", 12F);
            btnDeleteAll.Click += BtnDeleteAll_Click;
            headerPanel.Controls.Add(btnDeleteAll);

            btnInfo = CreateHeaderButton("ℹ", "btnInfo", 38);
            btnInfo.Font = new Font("Segoe UI Symbol", 12F, FontStyle.Regular);
            btnInfo.Click += BtnInfo_Click;
            headerPanel.Controls.Add(btnInfo);

            // --- ListView Setup ---
            lvTasks = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                MultiSelect = false,
                CheckBoxes = false,
                OwnerDraw = true,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                Font = new Font("Segoe UI", 10.5F)
            };

            lvImageList = new ImageList { ImageSize = new Size(1, 38) };
            lvTasks.SmallImageList = lvImageList;

            // <<< CHANGE: Column Setup Adjusted >>>
            lvTasks.Columns.Add(new ColumnHeader { Name = "ColCheck", Text = "", Width = 45 });         // Index 0
            lvTasks.Columns.Add(new ColumnHeader { Name = "ColTask", Text = "TASK", Width = -2 });         // Index 1 (Fills space)
            lvTasks.Columns.Add(new ColumnHeader { Name = "ColPriority", Text = "PRIORITY", Width = 80 });// Index 2
            lvTasks.Columns.Add(new ColumnHeader { Name = "ColReminder", Text = "REMINDER", Width = 100 });// Index 3 (Wider for text)
            lvTasks.Columns.Add(new ColumnHeader { Name = "ColDelete", Text = "", Width = 40 });         // Index 4 (Delete X)

            // Event Handlers
            lvTasks.DrawColumnHeader += LvTasks_DrawColumnHeader;
            lvTasks.DrawItem += LvTasks_DrawItem;
            lvTasks.DrawSubItem += LvTasks_DrawSubItem;
            lvTasks.MouseClick += LvTasks_MouseClick;
            lvTasks.MouseDoubleClick += LvTasks_MouseDoubleClick;
            lvTasks.KeyDown += LvTasks_KeyDown;
            lvTasks.Paint += LvTasks_Paint;
            // <<< CHANGE: Add MouseMove and MouseLeave for hover effect (optional) >>>
            lvTasks.MouseMove += LvTasks_MouseMove;
            lvTasks.MouseLeave += LvTasks_MouseLeave;


            inputPanel = new Panel
            {
                Name = "inputPanel",
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(10, 10, 10, 10)
            };

            txtNewTask = new TextBox
            {
                Name = "txtNewTask",
                Dock = DockStyle.Fill,
                PlaceholderText = "Add new task...",
                Font = new Font("Segoe UI", 11F),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                Margin = new Padding(0, 0, 8, 0)
            };
            txtNewTask.KeyDown += TxtNewTask_KeyDown;

            btnAdd = new Button
            {
                Name = "btnAdd",
                Text = "+ ADD",
                Dock = DockStyle.Right,
                Width = 85,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = txtNewTask.Height
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            inputPanel.Controls.Add(txtNewTask);
            inputPanel.Controls.Add(btnAdd);

            this.Controls.Add(lvTasks);
            this.Controls.Add(inputPanel);
            this.Controls.Add(headerPanel);

            this.ActiveControl = txtNewTask;
        }

        private Button CreateHeaderButton(string text, string name, int width)
        {
            return new Button
            {
                Text = text,
                Name = name,
                Width = width,
                Height = headerPanel.Height - 14,
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Symbol", 11F),
                Margin = new Padding(0, 7, 6, 7),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = false,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private void InitializeTaskContextMenu()
        {
            // (ContextMenu remains the same as before)
            taskContextMenuStrip = new ContextMenuStrip();

            var editItem = new ToolStripMenuItem("Edit Task", null, TaskContextMenu_Edit_Click);
            var prioritySubMenu = new ToolStripMenuItem("Set Priority");
            prioritySubMenu.DropDownItems.Add("High", null, (s, e) => TaskContextMenu_SetPriority_Click(s, e, "High"));
            prioritySubMenu.DropDownItems.Add("Medium", null, (s, e) => TaskContextMenu_SetPriority_Click(s, e, "Medium"));
            prioritySubMenu.DropDownItems.Add("Low", null, (s, e) => TaskContextMenu_SetPriority_Click(s, e, "Low"));
            prioritySubMenu.DropDownItems.Add("-"); // Separator
            prioritySubMenu.DropDownItems.Add("None", null, (s, e) => TaskContextMenu_SetPriority_Click(s, e, "None"));

            var reminderSubMenu = new ToolStripMenuItem("Set Reminder");
            reminderSubMenu.DropDownItems.Add("In 1 Hour", null, (s, e) => TaskContextMenu_SetReminder_Click(s, e, TimeSpan.FromHours(1)));
            reminderSubMenu.DropDownItems.Add("In 2 Hours", null, (s, e) => TaskContextMenu_SetReminder_Click(s, e, TimeSpan.FromHours(2)));
            // Modify these lines:
            reminderSubMenu.DropDownItems.Add("Tomorrow Morning (9 AM)", null,
                (s, e) => TaskContextMenu_SetReminder_Click(s, e, GetTomorrowAt(9) - DateTime.Now));

            reminderSubMenu.DropDownItems.Add("Tomorrow Evening (6 PM)", null,
                (s, e) => TaskContextMenu_SetReminder_Click(s, e, GetTomorrowAt(18) - DateTime.Now));

            reminderSubMenu.DropDownItems.Add("-");
            reminderSubMenu.DropDownItems.Add("At Specific Time...", null, TaskContextMenu_SetSpecificTime_Click);
            reminderSubMenu.DropDownItems.Add("-");
            reminderSubMenu.DropDownItems.Add("Clear Reminder", null, TaskContextMenu_ClearReminder_Click);


            var deleteItem = new ToolStripMenuItem("Delete Task", null, TaskContextMenu_Delete_Click);
            var separator = new ToolStripSeparator();

            taskContextMenuStrip.Items.AddRange(new ToolStripItem[] {
                editItem,
                prioritySubMenu,
                reminderSubMenu,
                separator,
                deleteItem
            });

            taskContextMenuStrip.Opening += TaskContextMenuStrip_Opening;
            lvTasks.ContextMenuStrip = taskContextMenuStrip;
        }

        private void InitializeTray()
        {
            // (Tray initialization remains the same)
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show", null, (s, e) => ShowApp());
            trayMenu.Items.Add("-"); // Separator
            trayMenu.Items.Add("Exit", null, (s, e) => { reallyClosing = true; Application.Exit(); });

            trayIcon = new NotifyIcon
            {
                // Icon = Properties.Resources.AppIcon, // TODO: Use your app icon
                Icon = SystemIcons.Information, // Placeholder
                ContextMenuStrip = trayMenu,
                Visible = false, // Initially hidden, show when minimized
                Text = "Simple To-Do List"
            };
            trayIcon.DoubleClick += (s, e) => ShowApp();
        }

        private void InitializeReminderTimer()
        {
            // (Timer initialization remains the same)
            reminderTimer = new System.Windows.Forms.Timer();
            reminderTimer.Interval = REMINDER_TIMER_INTERVAL_MS;
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();
        }

        // --- Theming ---

        private void ApplyTheme()
        {
            // (Theme colors setup remains largely the same)
            darkTheme = appConfig.DarkTheme;

            if (darkTheme)
            {
                formBackColor = Color.FromArgb(37, 37, 38);
                headerColor1 = Color.FromArgb(55, 55, 58); headerColor2 = Color.FromArgb(40, 40, 42);
                inputPanelBackColor = Color.FromArgb(45, 45, 48);
                textBoxBackColor = Color.FromArgb(60, 60, 63);
                listForeColor = Color.FromArgb(220, 220, 220);
                listBackColorEven = Color.FromArgb(48, 48, 50); listBackColorOdd = Color.FromArgb(53, 53, 56);
                listSelectionBorderColor = Color.FromArgb(0, 122, 204);
                gridColor = Color.FromArgb(65, 65, 68);
                deleteIconColor = Color.FromArgb(180, 80, 80); // Darker red for dark theme
                deleteIconHoverColor = Color.FromArgb(220, 90, 90);
                btnTheme.BackColor = Color.FromArgb(70, 70, 73); btnTheme.ForeColor = Color.FromArgb(200, 200, 100);
                btnOptions.BackColor = Color.FromArgb(70, 70, 73); btnOptions.ForeColor = Color.FromArgb(210, 210, 210);
                btnDeleteAll.BackColor = Color.FromArgb(90, 40, 40); btnDeleteAll.ForeColor = Color.FromArgb(230, 150, 150);
                btnInfo.BackColor = Color.FromArgb(70, 70, 73); btnInfo.ForeColor = Color.FromArgb(150, 190, 230);
                btnAdd.BackColor = Color.FromArgb(60, 100, 60); btnAdd.ForeColor = Color.FromArgb(210, 255, 210);
                trayMenu.BackColor = formBackColor;
                trayMenu.ForeColor = listForeColor;
                foreach (ToolStripItem item in trayMenu.Items) { if (!(item is ToolStripSeparator)) { item.BackColor = trayMenu.BackColor; item.ForeColor = trayMenu.ForeColor; } }
            }
            else // Light Theme
            {
                formBackColor = Color.FromArgb(245, 245, 245);
                headerColor1 = Color.FromArgb(210, 210, 210); headerColor2 = Color.FromArgb(190, 190, 190);
                inputPanelBackColor = Color.FromArgb(230, 230, 230);
                textBoxBackColor = Color.White;
                listForeColor = Color.FromArgb(20, 20, 20);
                listBackColorEven = Color.White; listBackColorOdd = Color.FromArgb(248, 248, 250);
                listSelectionBorderColor = SystemColors.Highlight;
                gridColor = Color.FromArgb(225, 225, 225);
                deleteIconColor = Color.IndianRed; // Standard red for light theme
                deleteIconHoverColor = Color.Red;
                btnTheme.BackColor = Color.FromArgb(100, 100, 120); btnTheme.ForeColor = Color.White;
                btnOptions.BackColor = Color.FromArgb(190, 190, 190); btnOptions.ForeColor = Color.FromArgb(50, 50, 50);
                btnDeleteAll.BackColor = Color.FromArgb(211, 47, 47); btnDeleteAll.ForeColor = Color.White;
                btnInfo.BackColor = Color.FromArgb(190, 190, 190); btnInfo.ForeColor = Color.FromArgb(0, 122, 204);
                btnAdd.BackColor = Color.FromArgb(76, 175, 80); btnAdd.ForeColor = Color.White;
                trayMenu.BackColor = SystemColors.Control;
                trayMenu.ForeColor = SystemColors.ControlText;
                foreach (ToolStripItem item in trayMenu.Items) { if (!(item is ToolStripSeparator)) { item.BackColor = SystemColors.Control; item.ForeColor = SystemColors.ControlText; } }
            }

            this.BackColor = formBackColor;
            headerPanel.Tag = new Tuple<Color, Color>(headerColor1, headerColor2);
            headerPanel.Invalidate();
            inputPanel.BackColor = inputPanelBackColor;
            txtNewTask.BackColor = textBoxBackColor;
            txtNewTask.ForeColor = listForeColor;
            btnTheme.Text = darkTheme ? "☀" : "🌙";

            lvTasks.BackColor = listBackColorEven;
            lvTasks.ForeColor = listForeColor;
            lvTasks.Invalidate();
        }

        private void HeaderPanel_Paint(object? sender, PaintEventArgs e)
        {
            // (Header painting remains the same)
            if (headerPanel.Tag is Tuple<Color, Color> colors) { using (var brush = new LinearGradientBrush(headerPanel.ClientRectangle, colors.Item1, colors.Item2, LinearGradientMode.Vertical)) { e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle); } }
            Color bottomBorder = darkTheme ? Color.FromArgb(60, 60, 63) : Color.FromArgb(180, 180, 180); using (var pen = new Pen(bottomBorder, 1)) { e.Graphics.DrawLine(pen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1); }
        }

        // --- Drawing Handlers ---

        private void LvTasks_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            // (Column header drawing remains the same, indices checked below)
            Color headerBackColor = darkTheme ? Color.FromArgb(55, 55, 58) : Color.FromArgb(225, 225, 225);
            Color headerTextColor = darkTheme ? Color.FromArgb(210, 210, 210) : Color.FromArgb(60, 60, 60);
            Color headerBorderColor = darkTheme ? Color.FromArgb(75, 75, 78) : Color.FromArgb(200, 200, 200);
            Font headerFont = new Font("Segoe UI Semibold", 9F);

            using (var backBrush = new SolidBrush(headerBackColor)) { e.Graphics.FillRectangle(backBrush, e.Bounds); }

            using (var sf = new StringFormat())
            {
                // <<< CHANGE: Adjust alignment check for new column indices >>>
                sf.Alignment = (e.ColumnIndex == 0 || e.ColumnIndex == 4) ? StringAlignment.Center : StringAlignment.Near; // Checkbox (0) and Delete (4) centered
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                Rectangle textBounds = e.Bounds;
                if (sf.Alignment == StringAlignment.Near) { textBounds.X += 8; textBounds.Width -= 10; } else { textBounds.X += 2; textBounds.Width -= 4; }
                textBounds.Y += 1;

                using (var textBrush = new SolidBrush(headerTextColor)) { if (!string.IsNullOrEmpty(e.Header.Text)) { e.Graphics.DrawString(e.Header.Text, headerFont, textBrush, textBounds, sf); } }
            }

            using (var linePen = new Pen(headerBorderColor)) { e.Graphics.DrawLine(linePen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1); if (e.ColumnIndex < lvTasks.Columns.Count - 1) { e.Graphics.DrawLine(linePen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom - 1); } }
            headerFont.Dispose();
        }

        private void LvTasks_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            // Keep minimal
        }

        private void LvTasks_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            TaskItem? taskItem = e.Item.Tag as TaskItem;
            if (taskItem == null) return;

            bool isSelected = e.Item.Selected;
            bool isChecked = taskItem.IsChecked;
            bool isOddRow = e.Item.Index % 2 != 0;
            // <<< CHANGE: Removed hasReminder flag here, handle in column drawing >>>

            // Background
            Color backColor = isOddRow ? listBackColorOdd : listBackColorEven;
            if (isSelected) { backColor = darkTheme ? Color.FromArgb(60, 65, 75) : Color.FromArgb(220, 235, 250); }
            using (SolidBrush backBrush = new SolidBrush(backColor)) { e.Graphics.FillRectangle(backBrush, e.Bounds); }

            // Grid Lines (Bottom only)
            using (var linePen = new Pen(gridColor)) { e.Graphics.DrawLine(linePen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1); }

            // Selection Border
            if (isSelected && lvTasks.Focused) { using (Pen borderPen = new Pen(listSelectionBorderColor, 1)) { Rectangle borderRect = e.Item.Bounds; if (e.ColumnIndex == 0) { borderRect.Width -= 1; borderRect.Height -= 1; e.Graphics.DrawRectangle(borderPen, borderRect); } } }

            // Content Drawing Setup
            StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip, Trimming = StringTrimming.EllipsisCharacter };
            Rectangle contentBounds = e.Bounds;
            int generalPadding = 8;
            contentBounds.X += generalPadding;
            contentBounds.Width = Math.Max(0, contentBounds.Width - (generalPadding * 2));

            // <<< CHANGE: Column Drawing Logic with Updated Indices >>>
            switch (e.ColumnIndex)
            {
                case 0: // Checkbox Column (Index 0)
                    var checkBoxSize = 18;
                    var checkBounds = new Rectangle(e.Bounds.Left + (e.Bounds.Width - checkBoxSize) / 2, e.Bounds.Top + (e.Bounds.Height - checkBoxSize) / 2, checkBoxSize, checkBoxSize);
                    var checkState = isChecked ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                    if (Application.RenderWithVisualStyles) { CheckBoxRenderer.DrawCheckBox(e.Graphics, checkBounds.Location, checkState); }
                    else { ControlPaint.DrawCheckBox(e.Graphics, checkBounds, isChecked ? ButtonState.Checked : ButtonState.Normal); }
                    break;

                case 1: // Task Text Column (Index 1)
                    var fontStyle = isChecked ? FontStyle.Strikeout | FontStyle.Italic : FontStyle.Regular;
                    Color textColor = isChecked ? (darkTheme ? Color.Gray : SystemColors.GrayText) : listForeColor;
                    sf.Alignment = StringAlignment.Near;
                    using (var font = new Font(lvTasks.Font, fontStyle))
                    using (var textBrush = new SolidBrush(textColor)) { e.Graphics.DrawString(taskItem.Text, font, textBrush, contentBounds, sf); }
                    break;

                case 2: // Priority Pill Column (Index 2)
                    DrawPriorityPill(e.Graphics, contentBounds, taskItem.Priority, listForeColor);
                    break;

                case 3: // Reminder Text Column (Index 3)
                    string reminderText = FormatReminderTime(taskItem.ReminderTime);
                    Color reminderColor = taskItem.ReminderTime.HasValue ? listForeColor : (darkTheme ? Color.DarkGray : SystemColors.GrayText); // Dim "No reminder"
                    // Check if reminder is past due (optional styling)
                    if (taskItem.ReminderTime.HasValue && taskItem.ReminderTime.Value < DateTime.Now)
                    {
                        // reminderColor = Color.OrangeRed; // Or some other indication
                        // Could also add strikethrough to reminderText here if desired
                    }
                    sf.Alignment = StringAlignment.Near; // Left align text
                    using (var font = new Font(lvTasks.Font.FontFamily, lvTasks.Font.Size - 0.5f)) // Slightly smaller font maybe
                    using (var textBrush = new SolidBrush(reminderColor)) { e.Graphics.DrawString(reminderText, font, textBrush, contentBounds, sf); }
                    break;

                case 4: // Delete 'X' Column (Index 4)
                    sf.Alignment = StringAlignment.Center;
                    Font iconFont = new Font("Segoe UI Symbol", 11F); // Adjust size as needed
                    // Use hover color if mouse is over this specific subitem (optional)
                    Color currentDeleteColor = (e.SubItem == hoveredDeleteSubItem) ? deleteIconHoverColor : deleteIconColor;
                    using (var textBrush = new SolidBrush(currentDeleteColor))
                    {
                        // Use e.Bounds for centering in the column cell
                        e.Graphics.DrawString("✖", iconFont, textBrush, e.Bounds, sf);
                    }
                    iconFont.Dispose();
                    break;
            }
            sf.Dispose();
        }

        // Helper to format reminder time for display
        private string FormatReminderTime(DateTime? reminderTime)
        {
            if (!reminderTime.HasValue)
            {
                return "No reminder";
            }

            DateTime reminder = reminderTime.Value;
            DateTime now = DateTime.Now;
            DateTime today = now.Date;
            DateTime tomorrow = today.AddDays(1);

            if (reminder.Date == today)
            {
                return $"Today {reminder:h:mm tt}"; // e.g., Today 3:30 PM
            }
            else if (reminder.Date == tomorrow)
            {
                return $"Tomorrow {reminder:h:mm tt}"; // e.g., Tomorrow 9:00 AM
            }
            else if (reminder.Year == today.Year)
            {
                return reminder.ToString("ddd MMM d, h:mm tt"); // e.g., Wed Aug 21, 10:00 AM
            }
            else
            {
                return reminder.ToString("g"); // Short date/time e.g., 08/21/2024 10:00 AM
            }
        }


        private void LvTasks_Paint(object? sender, PaintEventArgs e)
        {
            // (Paint handler for empty space remains the same)
            if (!lvTasks.OwnerDraw || lvTasks.View != View.Details || lvTasks.Items.Count == 0) return;
            Rectangle lastItemBounds; try { lastItemBounds = lvTasks.GetItemRect(lvTasks.Items.Count - 1); } catch (ArgumentOutOfRangeException) { return; }
            int y = lastItemBounds.Bottom; Rectangle clientRect = lvTasks.ClientRectangle;
            if (y < clientRect.Height) { using (var pen = new Pen(gridColor)) { while (y < clientRect.Height) { e.Graphics.DrawLine(pen, clientRect.Left, y - 1, clientRect.Right, y - 1); y += lvImageList.ImageSize.Height; } } }
        }

        private void DrawPriorityPill(Graphics g, Rectangle bounds, string priorityText, Color defaultTextColor)
        {
            // (Priority Pill drawing remains the same)
            Color baseColor, textColor; switch (priorityText) { case "Low": baseColor = priorityColorLow; textColor = darkTheme ? Color.WhiteSmoke : Color.DarkSlateGray; break; case "Medium": baseColor = priorityColorMedium; textColor = darkTheme ? Color.WhiteSmoke : Color.SaddleBrown; break; case "High": baseColor = priorityColorHigh; textColor = Color.White; break; default: baseColor = darkTheme ? Color.FromArgb(80, 80, 80) : Color.LightGray; textColor = defaultTextColor; priorityText = "None"; break; }
            int vertPadding = 6; int horzPadding = 10; int cornerRadius = (bounds.Height - (2 * vertPadding)) / 2;
            Rectangle pillRect = new Rectangle(bounds.Left + (bounds.Width - (bounds.Width - horzPadding * 2 + 4)) / 2, bounds.Top + vertPadding, bounds.Width - horzPadding * 2 + 4, bounds.Height - (2 * vertPadding));
            pillRect.Width = Math.Max(10, pillRect.Width); pillRect.Height = Math.Max(10, pillRect.Height); cornerRadius = Math.Max(1, Math.Min(cornerRadius, Math.Min(pillRect.Width, pillRect.Height) / 2));
            using (GraphicsPath path = GetRoundRectangle(pillRect, cornerRadius)) { g.SmoothingMode = SmoothingMode.AntiAlias; using (var brush = new SolidBrush(baseColor)) { g.FillPath(brush, path); } g.SmoothingMode = SmoothingMode.Default; }
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.None, FormatFlags = StringFormatFlags.NoWrap }) using (var textBrush = new SolidBrush(textColor)) using (var font = new Font(lvTasks.Font.FontFamily, lvTasks.Font.Size - 1.5f, FontStyle.Regular)) { RectangleF textRect = pillRect; g.DrawString(priorityText, font, textBrush, textRect, sf); }
        }

        private static GraphicsPath GetRoundRectangle(Rectangle bounds, int radius)
        {
            // (Rounded rectangle helper remains the same)
            GraphicsPath path = new GraphicsPath(); if (radius <= 0) { path.AddRectangle(bounds); return path; }
            int diameter = radius * 2; Rectangle arcRect = new Rectangle(bounds.Location, new Size(diameter, diameter)); path.AddArc(arcRect, 180, 90); arcRect.X = bounds.Right - diameter; path.AddArc(arcRect, 270, 90); arcRect.Y = bounds.Bottom - diameter; path.AddArc(arcRect, 0, 90); arcRect.X = bounds.Left; path.AddArc(arcRect, 90, 90); path.CloseFigure(); return path;
        }

        // --- Interaction Handlers ---

        private void TxtNewTask_KeyDown(object? sender, KeyEventArgs e)
        {
            // (New task input remains the same)
            if (e.KeyCode == Keys.Enter) { BtnAdd_Click(sender, e); e.SuppressKeyPress = true; }
        }

        private void LvTasks_KeyDown(object? sender, KeyEventArgs e)
        {
            // (Key handling remains the same)
            if (e.KeyCode == Keys.Delete && lvTasks.SelectedItems.Count > 0) { TaskContextMenu_Delete_Click(sender, e); }
            else if (e.KeyCode == Keys.F2 && lvTasks.SelectedItems.Count > 0) { var item = lvTasks.SelectedItems[0]; ShowTaskEditor(item, item.SubItems[1]); }
            else if (e.KeyCode == Keys.Space && lvTasks.SelectedItems.Count > 0) { ToggleTaskChecked(lvTasks.SelectedItems[0]); e.SuppressKeyPress = true; }
        }

        private void LvTasks_MouseClick(object? sender, MouseEventArgs e)
        {
            if (isEditing) return;
            var hit = lvTasks.HitTest(e.Location);
            if (hit.Item == null) return;

            // <<< CHANGE: Use extension method reliably >>>
            int columnIndex = -1;
            // Find the column index based on the SubItem hit
            for (int i = 0; i < hit.Item.SubItems.Count; i++)
            {
                if (hit.Item.SubItems[i] == hit.SubItem)
                {
                    if (i < lvTasks.Columns.Count)
                    {
                        columnIndex = i;
                        break;
                    }
                }
            }
            // Fallback check based on X coordinate if SubItem mapping failed (less reliable)
            // if (columnIndex == -1) {
            //     ColumnHeader column = hit.Item.ListView.GetColumnHeaderAt(e.X, e.Y); // Assumes extension method exists
            //     if (column != null) columnIndex = column.Index;
            // }

            if (columnIndex == -1) return; // Couldn't determine column


            if (e.Button == MouseButtons.Left)
            {
                if (columnIndex == 0) // Checkbox column (Index 0)
                {
                    ToggleTaskChecked(hit.Item);
                }
                // <<< CHANGE: Add click handler for Delete Column (Index 4) >>>
                else if (columnIndex == 4) // Delete column (Index 4)
                {
                    // Add a small buffer zone check if needed, or just check if column was hit
                    // if (hit.SubItem.Bounds.Contains(e.Location)) // Check bounds precisely? Might be overkill
                    DeleteTask(hit.Item); // Call delete method (handles confirmation)
                }
            }
            // Right click handled by ContextMenuStrip
        }

        private void LvTasks_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            // <<< CHANGE: Double click indices updated >>>
            if (isEditing) return;
            var hit = lvTasks.HitTest(e.Location);
            if (hit.Item == null || hit.SubItem == null) return;

            int columnIndex = -1;
            for (int i = 0; i < hit.Item.SubItems.Count; i++) { if (hit.Item.SubItems[i] == hit.SubItem && i < lvTasks.Columns.Count) { columnIndex = i; break; } }
            if (columnIndex == -1) return;

            if (columnIndex == 1) // Task Text (Index 1)
            {
                ShowTaskEditor(hit.Item, hit.SubItem);
            }
            else if (columnIndex == 2) // Priority (Index 2)
            {
                suppressNextItemCheck = true;
                CyclePriority(hit.Item);
            }
            // Optional: Double click reminder to edit/set?
            // else if (columnIndex == 3) // Reminder (Index 3)
            // {
            //     TaskContextMenu_SetSpecificTime_Click(null, EventArgs.Empty); // Simulate context menu click
            // }
        }

        // <<< CHANGE: Handlers for Delete Icon Hover Effect (Optional) >>>
        private void LvTasks_MouseMove(object? sender, MouseEventArgs e)
        {
            var hit = lvTasks.HitTest(e.Location);
            ListViewItem.ListViewSubItem? currentHover = null;

            if (hit.Item != null && hit.SubItem != null)
            {
                int columnIndex = -1;
                for (int i = 0; i < hit.Item.SubItems.Count; i++) { if (hit.Item.SubItems[i] == hit.SubItem && i < lvTasks.Columns.Count) { columnIndex = i; break; } }

                if (columnIndex == 4)
                { // If over the delete column (Index 4)
                    currentHover = hit.SubItem;
                }
            }

            // Check if hover state changed
            if (hoveredDeleteSubItem != currentHover)
            {
                ListViewItem.ListViewSubItem? previousHover = hoveredDeleteSubItem;
                hoveredDeleteSubItem = currentHover;

            // Invalidate the old and new hover items to force redraw
            if (previousHover != null && hit.Item != null)  // Use hit.Item instead of SubItem.ListViewItem
            {
                try { lvTasks.Invalidate(previousHover.Bounds); } catch { }
            }
            if (hoveredDeleteSubItem != null && hit.Item != null)
            {
                try { lvTasks.Invalidate(hoveredDeleteSubItem.Bounds); } catch { }
            }
            }
        }

        private void LvTasks_MouseLeave(object? sender, EventArgs e)
        {
            // Clear hover state when mouse leaves the ListView
            if (hoveredDeleteSubItem != null)
            {
                ListViewItem.ListViewSubItem previousHover = hoveredDeleteSubItem;
                hoveredDeleteSubItem = null;

                // Use bounds directly without checking ListViewItem
                try { lvTasks.Invalidate(previousHover.Bounds); } catch { }
            }
        }


        // --- Task Management Methods ---

        private void AddNewTask(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            var newTask = new TaskItem { Text = text.Trim(), Priority = "None", IsChecked = false, ReminderTime = null };
            taskList.Add(newTask);

            // <<< CHANGE: Add empty strings for Reminder Text and Delete columns >>>
            var lvi = new ListViewItem(new[] { "", newTask.Text, newTask.Priority, "", "" }) // 5 columns now
            {
                Checked = false,
                Tag = newTask
            };

            lvTasks.Items.Add(lvi);
            EnsureItemVisible(lvi);
            UpdateTaskVisuals(lvi); // Explicitly update visuals including reminder text
            SaveTasks();
        }

        private void ToggleTaskChecked(ListViewItem lvi)
        {
            // (Remains the same)
            if (lvi == null || !(lvi.Tag is TaskItem task)) return;
            task.IsChecked = !task.IsChecked;
            UpdateTaskVisuals(lvi);
            SaveTasks();
        }

        private void UpdateTaskData(ListViewItem lvi, string? newText = null, string? newPriority = null, DateTime? newReminderTime = null, bool reminderCleared = false)
        {
            // (Remains mostly the same, but updates reminder subitem text)
            if (lvi == null || !(lvi.Tag is TaskItem task)) return;

            bool changed = false;
            if (newText != null && task.Text != newText) { task.Text = newText; lvi.SubItems[1].Text = newText; changed = true; } // Index 1 = Task
            if (newPriority != null && task.Priority != newPriority) { task.Priority = newPriority; lvi.SubItems[2].Text = newPriority; changed = true; } // Index 2 = Priority

            // Handle reminder changes
            DateTime? oldReminder = task.ReminderTime;
            if (reminderCleared) { if (task.ReminderTime.HasValue) { task.ReminderTime = null; changed = true; } }
            else if (newReminderTime.HasValue) { if (!task.ReminderTime.HasValue || task.ReminderTime.Value != newReminderTime.Value) { task.ReminderTime = newReminderTime; changed = true; } }

            // <<< CHANGE: Update reminder text subitem if changed >>>
            if (task.ReminderTime != oldReminder)
            {
                lvi.SubItems[3].Text = FormatReminderTime(task.ReminderTime); // Index 3 = Reminder
                changed = true; // Ensure changed is true if only reminder text needed update
            }


            if (changed)
            {
                UpdateTaskVisuals(lvi); // Redraws the entire item
                SaveTasks();
            }
        }

        private void DeleteTask(ListViewItem lvi)
        {
            // (Delete logic remains the same)
            if (lvi == null || !(lvi.Tag is TaskItem task)) return;
            if (appConfig.ConfirmDelete && MessageBox.Show($"Delete task: '{task.Text}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) { return; }
            taskList.Remove(task);
            lvi.Remove();
            SaveTasks();
        }

        private void DeleteAllTasks()
        {
            // (Delete All logic remains the same)
            if (taskList.Count == 0) { MessageBox.Show("There are no tasks to delete.", "Empty List", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (appConfig.ConfirmDelete && MessageBox.Show("Are you sure you want to delete ALL tasks?", "Confirm Delete All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) { return; }
            taskList.Clear(); lvTasks.Items.Clear(); SaveTasks();
        }

        private void CyclePriority(ListViewItem lvi)
        {
            // (Cycle priority logic remains the same)
            if (lvi == null || !(lvi.Tag is TaskItem task)) return; string currentPriority = task.Priority; string nextPriority; switch (currentPriority) { case "None": nextPriority = "Low"; break; case "Low": nextPriority = "Medium"; break; case "Medium": nextPriority = "High"; break; case "High": nextPriority = "None"; break; default: nextPriority = "None"; break; }
            UpdateTaskData(lvi, newPriority: nextPriority);
        }

        // --- Inline Editors ---

        private void ShowTaskEditor(ListViewItem item, ListViewItem.ListViewSubItem subItem)
        {
            // (Task editor remains the same, uses column index 1)
            if (isEditing || item == null || subItem == null || !(item.Tag is TaskItem task)) return;
            isEditing = true;
            Rectangle itemBounds = item.Bounds; Rectangle subItemBounds = item.SubItems[1].Bounds; // Index 1 = Task
            Rectangle editorBounds = new Rectangle(itemBounds.Left + subItemBounds.Left, itemBounds.Top, subItemBounds.Width, itemBounds.Height);
            var tb = new TextBox { Text = task.Text, Bounds = editorBounds, Font = new Font(lvTasks.Font.FontFamily, lvTasks.Font.Size, FontStyle.Regular), BorderStyle = BorderStyle.FixedSingle, Tag = item, BackColor = textBoxBackColor, ForeColor = listForeColor, MaxLength = 250 };
            tb.LostFocus += (s, args) => FinishEditing(tb, false);
            tb.KeyDown += (s, args) => { if (args.KeyCode == Keys.Enter) { FinishEditing(tb, true); args.SuppressKeyPress = true; } else if (args.KeyCode == Keys.Escape) { FinishEditing(tb, false); args.SuppressKeyPress = true; } };
            lvTasks.Controls.Add(tb); tb.Focus(); tb.SelectAll();
        }

        private void FinishEditing(Control editorControl, bool saveData)
        {
            // (Finish editing logic remains the same)
            if (!isEditing || !lvTasks.Controls.Contains(editorControl)) return;
            ListViewItem? item = editorControl.Tag as ListViewItem; if (item == null || !(item.Tag is TaskItem task) || !lvTasks.Items.Contains(item)) { this.BeginInvoke((MethodInvoker)delegate { if (lvTasks.Controls.Contains(editorControl)) { lvTasks.Controls.Remove(editorControl); editorControl.Dispose(); } isEditing = false; }); return; }
            if (saveData) { if (editorControl is TextBox tb) { string newText = tb.Text.Trim(); if (!string.IsNullOrWhiteSpace(newText) && task.Text != newText) { UpdateTaskData(item, newText: newText); } } }
            this.BeginInvoke((MethodInvoker)delegate { if (lvTasks.Controls.Contains(editorControl)) { lvTasks.Controls.Remove(editorControl); editorControl.Dispose(); } isEditing = false; if (!lvTasks.IsDisposed && !lvTasks.Disposing) { lvTasks.Focus(); UpdateTaskVisuals(item); } });
        }

        // --- Button Click Handlers ---
        // (Button handlers remain the same)
        private void BtnAdd_Click(object? sender, EventArgs e) { AddNewTask(txtNewTask.Text); txtNewTask.Clear(); txtNewTask.Focus(); }
        private void BtnTheme_Click(object? sender, EventArgs e) { appConfig.DarkTheme = !appConfig.DarkTheme; ApplyTheme(); SaveConfig(); }
        private void BtnDeleteAll_Click(object? sender, EventArgs e) { DeleteAllTasks(); }
        private void BtnOptions_Click(object? sender, EventArgs e) { using (var optionsForm = new OptionsForm(appConfig)) { if (optionsForm.ShowDialog(this) == DialogResult.OK) { SaveConfig(); } } }
        private void BtnInfo_Click(object? sender, EventArgs e) { string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3); string infoText = $@"Simple To-Do List v{version}\n\nFeatures:\n- Add, edit, prioritize, and check off tasks.\n- Set reminders for specific tasks (Right-click task -> Set Reminder).\n- Optional recurring reminders for 'Drink Water' and 'Stand Up' (Configure via ⚙ Options).\n- Light/Dark theme toggle (🌙/☀ button).\n- Minimizes to system tray on close.\n\nTips:\n- Double-click task text to edit.\n- Double-click priority to cycle (None -> Low -> Med -> High).\n- Use Spacebar to toggle task completion.\n- Use Delete key or click '✖' to delete task.\n- Right-click tasks for more options.\n\nData stored in: {Path.GetDirectoryName(todoFilePath)}\n"; MessageBox.Show(infoText, "About Simple To-Do List", MessageBoxButtons.OK, MessageBoxIcon.Information); }


        // --- Context Menu Handlers ---
        // (Context menu handlers remain the same)
        private void TaskContextMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e) { bool itemSelected = lvTasks.SelectedItems.Count > 0; foreach (ToolStripItem item in taskContextMenuStrip.Items) { item.Enabled = itemSelected; if (item.Text == "Clear Reminder" && itemSelected) { TaskItem? task = lvTasks.SelectedItems[0].Tag as TaskItem; item.Enabled = task?.ReminderTime.HasValue ?? false; } } }
        private void TaskContextMenu_Edit_Click(object? sender, EventArgs e) { if (lvTasks.SelectedItems.Count > 0) { var item = lvTasks.SelectedItems[0]; ShowTaskEditor(item, item.SubItems[1]); } } // Index 1 = Task
        private void TaskContextMenu_SetPriority_Click(object? sender, EventArgs e, string priority) { if (lvTasks.SelectedItems.Count > 0) { UpdateTaskData(lvTasks.SelectedItems[0], newPriority: priority); } }
        private void TaskContextMenu_SetReminder_Click(object? sender, EventArgs e, TimeSpan duration) { if (lvTasks.SelectedItems.Count > 0) { DateTime reminderTime = DateTime.Now.Add(duration); UpdateTaskData(lvTasks.SelectedItems[0], newReminderTime: reminderTime); } }
        private DateTime GetTomorrowAt(int hour) { DateTime tomorrow = DateTime.Today.AddDays(1); return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, hour, 0, 0); }
        private void TaskContextMenu_SetSpecificTime_Click(object? sender, EventArgs e) { if (lvTasks.SelectedItems.Count == 0) return; ListViewItem lvi = lvTasks.SelectedItems[0]; TaskItem? task = lvi.Tag as TaskItem; if (task == null) return; string defaultTime = task.ReminderTime.HasValue ? task.ReminderTime.Value.ToString("g") : ""; using (var inputBox = new InputBoxForm("Set Reminder Time", $"Enter reminder time for:\n'{task.Text}'", defaultTime)) { if (inputBox.ShowDialog(this) == DialogResult.OK && inputBox.ParsedDateTime.HasValue) { UpdateTaskData(lvi, newReminderTime: inputBox.ParsedDateTime.Value); } } }
        private void TaskContextMenu_ClearReminder_Click(object? sender, EventArgs e) { if (lvTasks.SelectedItems.Count > 0) { UpdateTaskData(lvTasks.SelectedItems[0], reminderCleared: true); } }
        private void TaskContextMenu_Delete_Click(object? sender, EventArgs e) { if (lvTasks.SelectedItems.Count > 0) { DeleteTask(lvTasks.SelectedItems[0]); } }

        // --- Reminder Timer Logic ---

        private void ReminderTimer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            bool tasksChanged = false;
            bool configChanged = false;


            // 1. Check Task Reminders
            List<TaskItem> triggeredTasks = new List<TaskItem>();
            List<ListViewItem> itemsToUpdate = new List<ListViewItem>(); // <<< Track items needing visual update

            foreach (ListViewItem lvi in lvTasks.Items)
            {
                if (lvi.Tag is TaskItem task && task.ReminderTime.HasValue && now >= task.ReminderTime.Value)
                {
                    triggeredTasks.Add(task);
                    task.ReminderTime = null; // Clear reminder data after triggering
                    tasksChanged = true;
                    itemsToUpdate.Add(lvi); // Mark LVI for update
                }
            }

            // Update visuals after iterating
            foreach (var lvi in itemsToUpdate)
            {
                UpdateTaskVisuals(lvi);
            }

            // Show notifications outside the loop
            foreach (var task in triggeredTasks)
            {
                ShowApp(); // Bring window to front
                MessageBox.Show(this, $"Reminder for task:\n\n{task.Text}", "Task Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // 2. Check Recurring Reminders
            // <<< CHANGE: Corrected Recurring Reminder Logic >>>

            // Water Reminder
            if (appConfig.EnableWaterReminder && appConfig.WaterReminderIntervalMinutes > 0)
            {
                DateTime? lastTime = appConfig.LastWaterReminderTime;

                // If first time running or just enabled, set the base time but don't trigger yet.
                if (!lastTime.HasValue)
                {
                    appConfig.LastWaterReminderTime = now;
                    lastTime = now; // Use 'now' as the base for the first interval calculation
                    configChanged = true;
                    // Don't proceed to check trigger on this first tick after enable/load
                }
                else // Only check trigger if a last time exists
                {
                    DateTime nextTriggerTime = lastTime.Value.AddMinutes(appConfig.WaterReminderIntervalMinutes);

                    if (now >= nextTriggerTime)
                    {
                        // Use balloon tip if minimized/hidden
                        if (!this.Visible)
                        {
                            trayIcon.ShowBalloonTip(3000, "Stay Hydrated", "Remember to drink some water!", ToolTipIcon.Info);
                        }
                        else
                        {
                            this.Activate(); // Ensure visible window is focused
                            MessageBox.Show(this, "Remember to drink some water!", "Stay Hydrated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        appConfig.LastWaterReminderTime = now; // Update last time triggered *after* showing notification
                        configChanged = true;
                    }
                }
            }


            // Stand Up Reminder (Similar corrected logic)
            if (appConfig.EnableStandUpReminder && appConfig.StandUpReminderIntervalMinutes > 0)
            {
                DateTime? lastTime = appConfig.LastStandUpReminderTime;

                if (!lastTime.HasValue)
                {
                    appConfig.LastStandUpReminderTime = now;
                    lastTime = now;
                    configChanged = true;
                }
                else
                {
                    DateTime nextTriggerTime = lastTime.Value.AddMinutes(appConfig.StandUpReminderIntervalMinutes);

                    if (now >= nextTriggerTime)
                    {
                        if (!this.Visible)
                        {
                            trayIcon.ShowBalloonTip(3000, "Take a Break", "Time to stand up, stretch, and look away!", ToolTipIcon.Info);
                        }
                        else
                        {
                            this.Activate();
                            MessageBox.Show(this, "Time to stand up, stretch, and look away from the screen!", "Take a Break", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        appConfig.LastStandUpReminderTime = now;
                        configChanged = true;
                    }
                }
            }

            // Save if changes occurred
            if (tasksChanged) SaveTasks();
            if (configChanged) SaveConfig();
        }


        // --- Data Handling & Config ---

        private void SaveTasks()
        {
            // (Save remains the same)
            try { var options = new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true }; File.WriteAllText(todoFilePath, JsonSerializer.Serialize(taskList, options)); } catch (Exception ex) { Console.WriteLine($"Error saving tasks: {ex.Message}"); }
        }

        private void LoadTasks()
        {
            // (Load remains the same)
            taskList.Clear(); if (!File.Exists(todoFilePath)) return; try { var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; var loadedTasks = JsonSerializer.Deserialize<List<TaskItem>>(File.ReadAllText(todoFilePath), options); if (loadedTasks != null) { taskList = loadedTasks; } } catch (JsonException jex) { MessageBox.Show($"Error reading tasks file '{Path.GetFileName(todoFilePath)}'. It might be corrupted.\n\nDetails: {jex.Message}\n\nA backup might be created if possible.", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error); TryBackupCorruptedFile(todoFilePath); } catch (Exception ex) { MessageBox.Show($"Error loading tasks: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void PopulateListView()
        {
            // (Populate list view needs to add reminder text subitem correctly)
            lvTasks.BeginUpdate();
            lvTasks.Items.Clear();
            foreach (var task in taskList)
            {
                // <<< CHANGE: Ensure 5 subitems are created, including reminder text >>>
                var lvi = new ListViewItem(new[] {
                     "",                             // Checkbox (Index 0)
                     task.Text ?? "",                // Task Text (Index 1)
                     task.Priority ?? "None",        // Priority (Index 2)
                     FormatReminderTime(task.ReminderTime), // Reminder Text (Index 3) <<< ADDED FORMATTED TEXT
                     ""                              // Delete Icon Placeholder (Index 4)
                 })
                {
                    Checked = task.IsChecked,
                    Tag = task
                };
                lvTasks.Items.Add(lvi);
                // UpdateTaskVisuals(lvi); // Called below implicitly by EndUpdate/redraw
            }
            lvTasks.EndUpdate();
        }

        private void UpdateTaskVisuals(ListViewItem lvi)
        {
            // (Update visuals needs to ensure reminder text is set)
            if (lvi == null || !(lvi.Tag is TaskItem task) || lvi.SubItems.Count <= 3) return; // Basic check

            // Update SubItem text fields directly
            lvi.SubItems[1].Text = task.Text;                     // Task Text
            lvi.SubItems[2].Text = task.Priority;                 // Priority
            lvi.SubItems[3].Text = FormatReminderTime(task.ReminderTime); // <<< SET REMINDER TEXT
            lvi.Checked = task.IsChecked;                         // Reflect check state

            // Force redraw
            if (lvTasks.IsHandleCreated && !lvTasks.Disposing) { try { lvTasks.Invalidate(lvi.Bounds); } catch { /* Ignore if item removed during process */ } }
        }


        private void SaveConfig()
        {
            // (Save Config remains the same)
            try { var options = new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true }; File.WriteAllText(configPath, JsonSerializer.Serialize(appConfig, options)); } catch (Exception ex) { Console.WriteLine($"Error saving config: {ex.Message}"); }
        }

        private void LoadConfig()
        {
            // (Load Config remains the same)
            appConfig = new Config(); if (File.Exists(configPath)) { try { var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; var loadedConfig = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath), options); if (loadedConfig != null) { appConfig = loadedConfig; } } catch (JsonException jex) { MessageBox.Show($"Error reading config file '{Path.GetFileName(configPath)}'. Using default settings.\n\nDetails: {jex.Message}", "Config Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); TryBackupCorruptedFile(configPath); appConfig = new Config(); } catch (Exception ex) { MessageBox.Show($"Error loading config: {ex.Message}. Using default settings.", "Config Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); appConfig = new Config(); } }
            darkTheme = appConfig.DarkTheme;
        }

        private void TryBackupCorruptedFile(string filePath)
        {
            // (Backup helper remains the same)
            try { string backupPath = filePath + ".corrupt." + DateTime.Now.ToString("yyyyMMddHHmmss"); File.Move(filePath, backupPath); MessageBox.Show($"The corrupted file has been backed up as:\n{Path.GetFileName(backupPath)}", "Backup Created", MessageBoxButtons.OK, MessageBoxIcon.Information); } catch (Exception ex) { MessageBox.Show($"Could not create a backup of the corrupted file.\n\nError: {ex.Message}", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // --- Window Behavior (Close to Tray, State Saving) ---
        // (OnFormClosing, ShowApp remain the same)
        protected override void OnFormClosing(FormClosingEventArgs e) { SaveConfig(); if (!reallyClosing && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; this.Hide(); trayIcon.Visible = true; } else { reminderTimer?.Stop(); trayIcon?.Dispose(); base.OnFormClosing(e); } }
        private void ShowApp() { this.Show(); if (this.WindowState == FormWindowState.Minimized) { this.WindowState = FormWindowState.Normal; } this.Activate(); trayIcon.Visible = false; }

        // --- Standard Methods & Cleanup ---
        private void InitializeComponent() { /* Not used if designing manually */ }
        protected override void Dispose(bool disposing) { if (disposing) { lvImageList?.Dispose(); trayIcon?.Dispose(); trayMenu?.Dispose(); taskContextMenuStrip?.Dispose(); reminderTimer?.Dispose(); } base.Dispose(disposing); }
        private void EnsureItemVisible(ListViewItem lvi) { if (lvi != null) { lvTasks.EnsureVisible(lvi.Index); } }

    } // End MainForm Class

    // Extension Method Helper (Keep this as is)
    public static class ListViewExtensions { public static ColumnHeader GetColumnHeaderAt(this ListView listView, int x, int y) { var hitTestInfo = listView.HitTest(x, y); if (hitTestInfo?.Item != null && hitTestInfo.SubItem != null) { for (int i = 0; i < hitTestInfo.Item.SubItems.Count; i++) { if (hitTestInfo.Item.SubItems[i] == hitTestInfo.SubItem) { if (i >= 0 && i < listView.Columns.Count) { return listView.Columns[i]; } } } } return null; } }

} // End Namespace