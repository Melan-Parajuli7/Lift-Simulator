using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ElevatorApp
{
    public partial class MainForm : Form
    {
        private const int LogRowsToShow = 500;

        private readonly LiftController _lift = new LiftController();
        private readonly SoundService _sound = new SoundService();

        private ILogRepository _logRepository;
        private bool _databaseErrorShown;
        private string _connectionInfo = "(app.config not read yet)";

        public MainForm()
        {
            InitializeComponent();

            _lift.Changed += ApplyState;
            _lift.Logged += WriteLog;
            _lift.MovementChanged += OnMovementChanged;
            _lift.EmergencyChanged += _sound.SetEmergency;
        }

        // ------------------------------------------------------------------ form events
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                var factory = new DbConnectionFactory();
                _connectionInfo = factory.ConnectionString;
                DbInitializer.EnsureCreated(factory);
                _logRepository = new LogRepository(factory);
            }
            catch (Exception ex)
            {
                ReportDatabaseError(ex);
            }

            WriteLog("Application started - lift at Ground Floor");
            ApplyState();
            _lift.Start();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _lift.Stop();
            _sound.Dispose();
            WriteLog("Application closed");
        }

        // ------------------------------------------------------------------ button events
        private void btnFloor1_Click(object sender, EventArgs e)
        {
            _lift.RequestFloor(Floor.First, "Cabin button 1 pressed");
        }

        private void btnFloorG_Click(object sender, EventArgs e)
        {
            _lift.RequestFloor(Floor.Ground, "Cabin button G pressed");
        }

        private void btnCallFirst_Click(object sender, EventArgs e)
        {
            _lift.RequestFloor(Floor.First, "Call button pressed at First Floor");
        }

        private void btnCallGround_Click(object sender, EventArgs e)
        {
            _lift.RequestFloor(Floor.Ground, "Call button pressed at Ground Floor");
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            _lift.PressOpen();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            _lift.PressClose();
        }

        private void btnEmergency_Click(object sender, EventArgs e)
        {
            _lift.ToggleEmergency();
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            if (_logRepository == null) return;

            try
            {
                _logRepository.Clear();
                RefreshLog();
            }
            catch (Exception ex)
            {
                ReportDatabaseError(ex);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ------------------------------------------------------------------ lift -> screen
        private void OnMovementChanged(bool moving)
        {
            _sound.SetMoving(moving);
            if (!moving)
                _sound.PlayDing();
        }

        /// <summary>Copies the lift state to the controls. Called every tick (about 33 times per second).</summary>
        private void ApplyState()
        {
            // safety sensor: cursor in the doorway of the door where the lift is standing
            DoorControl activeDoor = _lift.CurrentFloor == Floor.First ? doorFirst : doorGround;
            if (!_lift.IsMoving && activeDoor.IsCursorInDoorway)
                _lift.DoorwayBlocked();

            bool atFirst = !_lift.IsMoving && _lift.CurrentFloor == Floor.First;
            bool atGround = !_lift.IsMoving && _lift.CurrentFloor == Floor.Ground;
            string displayFloor = _lift.DisplayFloor.Label();

            doorFirst.LiftHere = atFirst;
            doorFirst.OpenAmount = atFirst ? _lift.DoorOpenAmount : 0;
            doorFirst.LiftArriving = _lift.IsMoving && _lift.MovingTo == Floor.First;
            doorFirst.IndicatorText = displayFloor;
            doorFirst.Direction = _lift.Direction;

            doorGround.LiftHere = atGround;
            doorGround.OpenAmount = atGround ? _lift.DoorOpenAmount : 0;
            doorGround.LiftArriving = _lift.IsMoving && _lift.MovingTo == Floor.Ground;
            doorGround.IndicatorText = displayFloor;
            doorGround.Direction = _lift.Direction;

            ledDisplay.FloorText = displayFloor;
            ledDisplay.StatusMessage = _lift.StatusText;
            ledDisplay.Direction = _lift.Direction;
            ledDisplay.Moving = _lift.IsMoving && !_lift.IsEmergency;
            ledDisplay.Emergency = _lift.IsEmergency;

            Floor? target = _lift.TargetFloor;
            bool toFirst = target.HasValue && target.Value == Floor.First;
            bool toGround = target.HasValue && target.Value == Floor.Ground;

            btnFloor1.Lit = toFirst;
            btnCallFirst.Lit = toFirst;
            btnFloorG.Lit = toGround;
            btnCallGround.Lit = toGround;
            btnOpen.Lit = _lift.DoorPhase == DoorPhase.Opening;
            btnClose.Lit = _lift.DoorPhase == DoorPhase.Closing;
            btnEmergency.Lit = _lift.IsEmergency;

            // blinking lamps / moving arrows need a repaint even when no value changed
            if (_lift.IsMoving || _lift.IsEmergency || target.HasValue)
            {
                ledDisplay.Invalidate();
                doorFirst.Invalidate();
                doorGround.Invalidate();
                btnEmergency.Invalidate();
            }
        }

        // ------------------------------------------------------------------ log (database)
        private void WriteLog(string action)
        {
            if (_logRepository == null) return;

            try
            {
                _logRepository.Add(action);
                RefreshLog();
            }
            catch (Exception ex)
            {
                ReportDatabaseError(ex);
            }
        }

        private void RefreshLog()
        {
            if (_logRepository == null) return;

            List<LogEntry> entries = _logRepository.GetRecent(LogRowsToShow);

            dgvLog.DataSource = entries
                .Select(entry => new
                {
                    ID = entry.Id,
                    Date = entry.Date.ToString("dd/MM/yyyy"),
                    Time = DateTime.Today.Add(entry.Time).ToString("hh:mm:ss tt"),
                    Action = entry.Action
                })
                .ToList();

            ConfigureLogColumns();

            if (dgvLog.RowCount > 0)
            {
                try { dgvLog.FirstDisplayedScrollingRowIndex = dgvLog.RowCount - 1; }
                catch (InvalidOperationException) { }
                dgvLog.ClearSelection();
            }
        }

        private void ConfigureLogColumns()
        {
            if (dgvLog.Columns.Count < 4) return;

            dgvLog.Columns["ID"].Width = 55;
            dgvLog.Columns["Date"].Width = 100;
            dgvLog.Columns["Time"].Width = 105;
            dgvLog.Columns["Action"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void ReportDatabaseError(Exception ex)
        {
            _logRepository = null;                 // stop trying - the lift still works without the log
            if (_databaseErrorShown) return;
            _databaseErrorShown = true;

            MessageBox.Show(
                "The activity log could not connect to SQL Server:\n\n" + ex.Message +
                "\n\nConnection string used by the app:\n" + _connectionInfo +
                "\n\nCheck the connection string in app.config.\n" +
                "For LocalDB use:  Server=(localdb)\\MSSQLLocalDB\n" +
                "The lift simulation still works, but nothing will be logged.",
                "Database error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}