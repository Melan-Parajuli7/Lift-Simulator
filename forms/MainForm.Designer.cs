using System.Drawing;
using System.Windows.Forms;

namespace ElevatorApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            doorFirst = new DoorControl();
            doorGround = new DoorControl();
            btnCallFirst = new PanelButton();
            btnCallGround = new PanelButton();
            panelMetal = new MetalPanel();
            ledDisplay = new LedDisplay();
            btnFloor1 = new PanelButton();
            btnFloorG = new PanelButton();
            btnClose = new PanelButton();
            btnOpen = new PanelButton();
            btnEmergency = new PanelButton();
            lblLogTitle = new Label();
            btnClearLog = new Button();
            btnExit = new Button();
            dgvLog = new DataGridView();
            panelMetal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
            SuspendLayout();
            //
            // doorFirst
            //
            doorFirst.Caption = "FIRST FLOOR";
            doorFirst.IndicatorText = "G";
            doorFirst.Location = new Point(150, 20);
            doorFirst.Name = "doorFirst";
            doorFirst.Size = new Size(230, 350);
            doorFirst.TabIndex = 0;
            doorFirst.TabStop = false;
            //
            // doorGround
            //
            doorGround.Caption = "GROUND FLOOR";
            doorGround.IndicatorText = "G";
            doorGround.Location = new Point(150, 400);
            doorGround.Name = "doorGround";
            doorGround.Size = new Size(230, 350);
            doorGround.TabIndex = 1;
            doorGround.TabStop = false;
            //
            // btnCallFirst  (call button on the first floor: lift please come down to me)
            //
            btnCallFirst.Glyph = PanelGlyph.ArrowDown;
            btnCallFirst.Location = new Point(400, 180);
            btnCallFirst.Look = PanelLook.Dark;
            btnCallFirst.Name = "btnCallFirst";
            btnCallFirst.Size = new Size(46, 46);
            btnCallFirst.TabIndex = 2;
            btnCallFirst.Click += btnCallFirst_Click;
            //
            // btnCallGround
            //
            btnCallGround.Glyph = PanelGlyph.ArrowUp;
            btnCallGround.Location = new Point(400, 590);
            btnCallGround.Look = PanelLook.Dark;
            btnCallGround.Name = "btnCallGround";
            btnCallGround.Size = new Size(46, 46);
            btnCallGround.TabIndex = 3;
            btnCallGround.Click += btnCallGround_Click;
            //
            // panelMetal
            //
            panelMetal.Controls.Add(ledDisplay);
            panelMetal.Controls.Add(btnFloor1);
            panelMetal.Controls.Add(btnFloorG);
            panelMetal.Controls.Add(btnClose);
            panelMetal.Controls.Add(btnOpen);
            panelMetal.Controls.Add(btnEmergency);
            panelMetal.Location = new Point(480, 20);
            panelMetal.Name = "panelMetal";
            panelMetal.Size = new Size(210, 730);
            panelMetal.TabIndex = 4;
            //
            // ledDisplay
            //
            ledDisplay.Location = new Point(20, 24);
            ledDisplay.Name = "ledDisplay";
            ledDisplay.Size = new Size(170, 300);
            ledDisplay.TabIndex = 0;
            ledDisplay.TabStop = false;
            //
            // btnFloor1
            //
            btnFloor1.Location = new Point(69, 346);
            btnFloor1.Name = "btnFloor1";
            btnFloor1.Size = new Size(72, 72);
            btnFloor1.TabIndex = 1;
            btnFloor1.Text = "1";
            btnFloor1.Click += btnFloor1_Click;
            //
            // btnFloorG
            //
            btnFloorG.Location = new Point(69, 430);
            btnFloorG.Name = "btnFloorG";
            btnFloorG.Size = new Size(72, 72);
            btnFloorG.TabIndex = 2;
            btnFloorG.Text = "G";
            btnFloorG.Click += btnFloorG_Click;
            //
            // btnClose
            //
            btnClose.Glyph = PanelGlyph.CloseDoors;
            btnClose.Location = new Point(26, 520);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(72, 72);
            btnClose.TabIndex = 3;
            btnClose.Click += btnClose_Click;
            //
            // btnOpen
            //
            btnOpen.Glyph = PanelGlyph.OpenDoors;
            btnOpen.Location = new Point(112, 520);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(72, 72);
            btnOpen.TabIndex = 4;
            btnOpen.Click += btnOpen_Click;
            //
            // btnEmergency
            //
            btnEmergency.Glyph = PanelGlyph.Emergency;
            btnEmergency.Location = new Point(55, 614);
            btnEmergency.Look = PanelLook.Alarm;
            btnEmergency.Name = "btnEmergency";
            btnEmergency.Size = new Size(100, 84);
            btnEmergency.TabIndex = 5;
            btnEmergency.Click += btnEmergency_Click;
            //
            // lblLogTitle
            //
            lblLogTitle.AutoSize = true;
            lblLogTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblLogTitle.Location = new Point(880, 26);
            lblLogTitle.Name = "lblLogTitle";
            lblLogTitle.Size = new Size(138, 28);
            lblLogTitle.TabIndex = 5;
            lblLogTitle.Text = "Activity Log";
            //
            // btnClearLog
            //
            btnClearLog.FlatStyle = FlatStyle.System;
            btnClearLog.Font = new Font("Segoe UI", 10F);
            btnClearLog.Location = new Point(720, 20);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(120, 40);
            btnClearLog.TabIndex = 6;
            btnClearLog.Text = "Clear Log";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            //
            // btnExit
            //
            btnExit.FlatStyle = FlatStyle.System;
            btnExit.Font = new Font("Segoe UI", 10F);
            btnExit.Location = new Point(1060, 20);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(120, 40);
            btnExit.TabIndex = 7;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            //
            // dgvLog
            //
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AllowUserToDeleteRows = false;
            dgvLog.AllowUserToResizeRows = false;
            dgvLog.BackgroundColor = Color.FromArgb(170, 170, 170);
            dgvLog.ColumnHeadersHeight = 34;
            dgvLog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvLog.Location = new Point(720, 74);
            dgvLog.MultiSelect = false;
            dgvLog.Name = "dgvLog";
            dgvLog.ReadOnly = true;
            dgvLog.RowHeadersVisible = false;
            dgvLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLog.Size = new Size(460, 676);
            dgvLog.TabIndex = 8;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1200, 770);
            Controls.Add(doorFirst);
            Controls.Add(doorGround);
            Controls.Add(btnCallFirst);
            Controls.Add(btnCallGround);
            Controls.Add(panelMetal);
            Controls.Add(lblLogTitle);
            Controls.Add(btnClearLog);
            Controls.Add(btnExit);
            Controls.Add(dgvLog);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Elevator";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            panelMetal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DoorControl doorFirst;
        private DoorControl doorGround;
        private PanelButton btnCallFirst;
        private PanelButton btnCallGround;
        private MetalPanel panelMetal;
        private LedDisplay ledDisplay;
        private PanelButton btnFloor1;
        private PanelButton btnFloorG;
        private PanelButton btnClose;
        private PanelButton btnOpen;
        private PanelButton btnEmergency;
        private Label lblLogTitle;
        private Button btnClearLog;
        private Button btnExit;
        private DataGridView dgvLog;
    }
}
