using System;
using System.Windows.Forms;

namespace TodoListApp
{
    public partial class OptionsForm : Form
    {
        private Config _currentConfig;

        // Controls (Declare manually or use Designer)
        private CheckBox chkConfirmDelete;
        private GroupBox grpRecurring;
        private CheckBox chkEnableWater;
        private NumericUpDown numWaterInterval;
        private Label lblWaterMinutes;
        private CheckBox chkEnableStandUp;
        private NumericUpDown numStandUpInterval;
        private Label lblStandUpMinutes;
        private Button btnOk;
        private Button btnCancel;
        private Label lblInfoRecurring; // Added info label


        public OptionsForm(Config config)
        {
            _currentConfig = config;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.chkConfirmDelete = new System.Windows.Forms.CheckBox();
            this.grpRecurring = new System.Windows.Forms.GroupBox();
            this.lblInfoRecurring = new System.Windows.Forms.Label();
            this.lblStandUpMinutes = new System.Windows.Forms.Label();
            this.numStandUpInterval = new System.Windows.Forms.NumericUpDown();
            this.chkEnableStandUp = new System.Windows.Forms.CheckBox();
            this.lblWaterMinutes = new System.Windows.Forms.Label();
            this.numWaterInterval = new System.Windows.Forms.NumericUpDown();
            this.chkEnableWater = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpRecurring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numStandUpInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWaterInterval)).BeginInit();
            this.SuspendLayout();
            //
            // chkConfirmDelete
            //
            this.chkConfirmDelete.AutoSize = true;
            this.chkConfirmDelete.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkConfirmDelete.Location = new System.Drawing.Point(22, 23);
            this.chkConfirmDelete.Name = "chkConfirmDelete";
            this.chkConfirmDelete.Size = new System.Drawing.Size(222, 21);
            this.chkConfirmDelete.TabIndex = 0;
            this.chkConfirmDelete.Text = "Confirm before deleting task(s)";
            this.chkConfirmDelete.UseVisualStyleBackColor = true;
            //
            // grpRecurring
            //
            this.grpRecurring.Controls.Add(this.lblInfoRecurring);
            this.grpRecurring.Controls.Add(this.lblStandUpMinutes);
            this.grpRecurring.Controls.Add(this.numStandUpInterval);
            this.grpRecurring.Controls.Add(this.chkEnableStandUp);
            this.grpRecurring.Controls.Add(this.lblWaterMinutes);
            this.grpRecurring.Controls.Add(this.numWaterInterval);
            this.grpRecurring.Controls.Add(this.chkEnableWater);
            this.grpRecurring.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpRecurring.Location = new System.Drawing.Point(16, 61);
            this.grpRecurring.Name = "grpRecurring";
            this.grpRecurring.Size = new System.Drawing.Size(355, 163);
            this.grpRecurring.TabIndex = 1;
            this.grpRecurring.TabStop = false;
            this.grpRecurring.Text = "Recurring Reminders";
            //
            // lblInfoRecurring
            //
            this.lblInfoRecurring.AutoSize = true;
            this.lblInfoRecurring.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfoRecurring.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblInfoRecurring.Location = new System.Drawing.Point(14, 137);
            this.lblInfoRecurring.Name = "lblInfoRecurring";
            this.lblInfoRecurring.Size = new System.Drawing.Size(268, 13);
            this.lblInfoRecurring.TabIndex = 6;
            this.lblInfoRecurring.Text = "Reminders trigger after the interval has passed.";
             //
            // lblStandUpMinutes
            //
            this.lblStandUpMinutes.AutoSize = true;
            this.lblStandUpMinutes.Enabled = false;
            this.lblStandUpMinutes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStandUpMinutes.Location = new System.Drawing.Point(273, 97);
            this.lblStandUpMinutes.Name = "lblStandUpMinutes";
            this.lblStandUpMinutes.Size = new System.Drawing.Size(50, 15);
            this.lblStandUpMinutes.TabIndex = 5;
            this.lblStandUpMinutes.Text = "minutes";
            //
            // numStandUpInterval
            //
            this.numStandUpInterval.Enabled = false;
            this.numStandUpInterval.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numStandUpInterval.Location = new System.Drawing.Point(198, 94);
            this.numStandUpInterval.Maximum = new decimal(new int[] { 1440, 0, 0, 0 }); // 24 hours
            this.numStandUpInterval.Minimum = new decimal(new int[] { 5, 0, 0, 0 }); // Sensible minimum
            this.numStandUpInterval.Name = "numStandUpInterval";
            this.numStandUpInterval.Size = new System.Drawing.Size(69, 23);
            this.numStandUpInterval.TabIndex = 4;
            this.numStandUpInterval.Value = new decimal(new int[] { 60, 0, 0, 0 }); // Default
            //
            // chkEnableStandUp
            //
            this.chkEnableStandUp.AutoSize = true;
            this.chkEnableStandUp.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkEnableStandUp.Location = new System.Drawing.Point(17, 95);
            this.chkEnableStandUp.Name = "chkEnableStandUp";
            this.chkEnableStandUp.Size = new System.Drawing.Size(175, 21);
            this.chkEnableStandUp.TabIndex = 3;
            this.chkEnableStandUp.Text = "Remind to stand up every";
            this.chkEnableStandUp.UseVisualStyleBackColor = true;
            this.chkEnableStandUp.CheckedChanged += new System.EventHandler(this.ChkEnableStandUp_CheckedChanged);
            //
            // lblWaterMinutes
            //
            this.lblWaterMinutes.AutoSize = true;
            this.lblWaterMinutes.Enabled = false;
            this.lblWaterMinutes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWaterMinutes.Location = new System.Drawing.Point(273, 51);
            this.lblWaterMinutes.Name = "lblWaterMinutes";
            this.lblWaterMinutes.Size = new System.Drawing.Size(50, 15);
            this.lblWaterMinutes.TabIndex = 2;
            this.lblWaterMinutes.Text = "minutes";
            //
            // numWaterInterval
            //
            this.numWaterInterval.Enabled = false;
            this.numWaterInterval.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numWaterInterval.Location = new System.Drawing.Point(198, 48);
            this.numWaterInterval.Maximum = new decimal(new int[] { 1440, 0, 0, 0 }); // 24 hours
            this.numWaterInterval.Minimum = new decimal(new int[] { 15, 0, 0, 0 }); // Sensible minimum
            this.numWaterInterval.Name = "numWaterInterval";
            this.numWaterInterval.Size = new System.Drawing.Size(69, 23);
            this.numWaterInterval.TabIndex = 1;
            this.numWaterInterval.Value = new decimal(new int[] { 120, 0, 0, 0 }); // Default
             //
            // chkEnableWater
            //
            this.chkEnableWater.AutoSize = true;
            this.chkEnableWater.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkEnableWater.Location = new System.Drawing.Point(17, 49);
            this.chkEnableWater.Name = "chkEnableWater";
            this.chkEnableWater.Size = new System.Drawing.Size(180, 21);
            this.chkEnableWater.TabIndex = 0;
            this.chkEnableWater.Text = "Remind to drink water every";
            this.chkEnableWater.UseVisualStyleBackColor = true;
            this.chkEnableWater.CheckedChanged += new System.EventHandler(this.ChkEnableWater_CheckedChanged);
            //
            // btnOk
            //
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.Location = new System.Drawing.Point(197, 237);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(84, 30);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(287, 237);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // OptionsForm
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(387, 281);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.grpRecurring);
            this.Controls.Add(this.chkConfirmDelete);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.grpRecurring.ResumeLayout(false);
            this.grpRecurring.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numStandUpInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWaterInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }


        private void LoadSettings()
        {
            chkConfirmDelete.Checked = _currentConfig.ConfirmDelete;

            chkEnableWater.Checked = _currentConfig.EnableWaterReminder;
            numWaterInterval.Value = Math.Max(numWaterInterval.Minimum, Math.Min(numWaterInterval.Maximum, _currentConfig.WaterReminderIntervalMinutes)); // Clamp value
            numWaterInterval.Enabled = chkEnableWater.Checked;
            lblWaterMinutes.Enabled = chkEnableWater.Checked;


            chkEnableStandUp.Checked = _currentConfig.EnableStandUpReminder;
            numStandUpInterval.Value = Math.Max(numStandUpInterval.Minimum, Math.Min(numStandUpInterval.Maximum, _currentConfig.StandUpReminderIntervalMinutes)); // Clamp value
            numStandUpInterval.Enabled = chkEnableStandUp.Checked;
            lblStandUpMinutes.Enabled = chkEnableStandUp.Checked;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            _currentConfig.ConfirmDelete = chkConfirmDelete.Checked;

            bool waterEnabled = chkEnableWater.Checked;
            bool standUpEnabled = chkEnableStandUp.Checked;

            // Reset last reminder time if reminder is disabled OR just enabled
            // So it doesn't trigger immediately if re-enabled after a long time.
            if (_currentConfig.EnableWaterReminder != waterEnabled)
            {
                 _currentConfig.LastWaterReminderTime = waterEnabled ? (DateTime?)DateTime.Now : null;
            }
             _currentConfig.EnableWaterReminder = waterEnabled;
            _currentConfig.WaterReminderIntervalMinutes = (int)numWaterInterval.Value;

            if (_currentConfig.EnableStandUpReminder != standUpEnabled)
            {
                 _currentConfig.LastStandUpReminderTime = standUpEnabled ? (DateTime?)DateTime.Now : null;
            }
            _currentConfig.EnableStandUpReminder = standUpEnabled;
            _currentConfig.StandUpReminderIntervalMinutes = (int)numStandUpInterval.Value;

            this.DialogResult = DialogResult.OK; // Set explicitly
            this.Close();
        }

        private void ChkEnableWater_CheckedChanged(object sender, EventArgs e)
        {
            numWaterInterval.Enabled = chkEnableWater.Checked;
            lblWaterMinutes.Enabled = chkEnableWater.Checked;
        }

        private void ChkEnableStandUp_CheckedChanged(object sender, EventArgs e)
        {
            numStandUpInterval.Enabled = chkEnableStandUp.Checked;
            lblStandUpMinutes.Enabled = chkEnableStandUp.Checked;
        }
    }
}