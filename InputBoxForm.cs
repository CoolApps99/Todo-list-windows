using System;
using System.Windows.Forms;
using System.Globalization;

namespace TodoListApp
{
    public partial class InputBoxForm : Form
    {
        // Controls (Declare manually or use Designer)
        private Label lblPrompt;
        private TextBox txtInput;
        private Button btnOk;
        private Button btnCancel;
        private Label lblExample; // Added Example Label

        public string InputValue { get; private set; } = string.Empty;
        public DateTime? ParsedDateTime { get; private set; } = null;


        public InputBoxForm(string title, string prompt, string defaultValue = "")
        {
            InitializeComponent();
            this.Text = title;
            lblPrompt.Text = prompt;
            txtInput.Text = defaultValue;
            lblExample.Text = $"Examples: {DateTime.Now:h:mm tt}, {DateTime.Now.AddHours(1):HH:mm}, Today 5pm, Tomorrow 9:30am";
            txtInput.Select();
        }


        private void InitializeComponent()
        {
            this.lblPrompt = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblExample = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblPrompt
            //
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrompt.Location = new System.Drawing.Point(12, 15);
            this.lblPrompt.MaximumSize = new System.Drawing.Size(360, 0); // Allow wrapping
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(54, 17);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "[Prompt]";
            //
            // txtInput
            //
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInput.Location = new System.Drawing.Point(15, 40); // Adjusted Y position
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(358, 25);
            this.txtInput.TabIndex = 1;
            //
            // btnOk
            //
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.Location = new System.Drawing.Point(217, 115); // Adjusted Y position
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 28);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(298, 115); // Adjusted Y position
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 28);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // lblExample
            //
            this.lblExample.AutoSize = true;
            this.lblExample.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExample.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblExample.Location = new System.Drawing.Point(12, 74); // Adjusted Y position
            this.lblExample.Name = "lblExample";
            this.lblExample.Size = new System.Drawing.Size(60, 13);
            this.lblExample.TabIndex = 2;
            this.lblExample.Text = "[Examples]";
            //
            // InputBoxForm
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(388, 155); // Adjusted Height
            this.Controls.Add(this.lblExample);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.lblPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputBoxForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "[Title]";
            this.ResumeLayout(false);
            this.PerformLayout();
        }


        private void BtnOk_Click(object sender, EventArgs e)
        {
            InputValue = txtInput.Text;
            // Attempt to parse the input string into a DateTime
            if (TryParseNaturalLanguageTime(InputValue, out DateTime reminderDateTime))
            {
                 // Ensure the parsed time is in the future
                if (reminderDateTime <= DateTime.Now)
                {
                     // If it's today but the time has passed, assume they meant tomorrow
                    if (reminderDateTime.Date == DateTime.Today)
                    {
                        reminderDateTime = reminderDateTime.AddDays(1);
                    } else {
                        // Or maybe it's just slightly in the past due to processing delay
                        // Add a small buffer, e.g., 1 minute. If still in past, prompt again or error.
                        if (reminderDateTime.AddMinutes(1) <= DateTime.Now) {
                            MessageBox.Show("Please enter a time in the future.", "Invalid Time", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return; // Keep the dialog open
                        }
                        // If within buffer, let it slide or adjust slightly? For simplicity, let's accept it if close.
                    }
                }
                ParsedDateTime = reminderDateTime;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Could not understand the time entered. Please try again.\nUse formats like '4:00 PM', '16:00', 'Tomorrow 9am'.", "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Keep the dialog open
            }
        }

        // Basic Natural Language Time Parsing (Can be expanded)
        private bool TryParseNaturalLanguageTime(string input, out DateTime result)
        {
            result = DateTime.MinValue;
            string timeString = input.Trim().ToLowerInvariant();
            DateTime baseDate = DateTime.Today;

            // Check for "tomorrow"
            if (timeString.Contains("tomorrow"))
            {
                baseDate = DateTime.Today.AddDays(1);
                timeString = timeString.Replace("tomorrow", "").Trim();
            }
             // Check for "today" (less critical as it's the default)
            else if (timeString.Contains("today"))
            {
                 timeString = timeString.Replace("today", "").Trim();
            }


            // Define common time formats
            string[] formats = {
                "h:mm tt", "h:mmtt", "hh:mm tt", "hh:mmtt", // 12-hour (e.g., 4:30 PM, 4:30PM)
                "H:mm", "HH:mm",                           // 24-hour (e.g., 16:30)
                "htt", "h tt",                             // Simple hour (e.g., 4pm, 4 pm)
                "H", "HH"                                  // Simple 24-hour (e.g., 16) - less reliable might need AM/PM assumption
            };

            foreach (string format in formats)
            {
                if (DateTime.TryParseExact(timeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
                {
                    result = baseDate.Add(parsedTime.TimeOfDay);
                    return true;
                }
            }

            // Fallback: Try general parsing (might misinterpret dates)
            if (DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out result))
            {
                 // General TryParse might return a full date/time far away, check if reasonable
                 if (result.Year == baseDate.Year || result.Year == baseDate.Year+1) // Allow current or next year only maybe?
                 {
                      // Check if only time was parsed, if so, combine with baseDate
                      if (result.Date == DateTime.MinValue.Date || result.Date == DateTime.Today) // Check if TryParse only picked up time part
                      {
                           result = baseDate.Add(result.TimeOfDay);
                      }
                     return true;
                 }
            }


            return false; // Failed to parse
        }
    }
}