using System;

namespace ElevatorApp
{
    /// <summary>
    /// The brain of the lift: moves the cabin between Ground and First floor, runs the door
    /// animation and handles the emergency state. It has no UI code - the form just listens
    /// to its events and reads its properties.
    /// </summary>
    public class LiftController
    {
        public const int TickMs = 30;

        private const double TravelMs = 5000;          // time to travel between the two floors
        private const double DoorMs = 1600;            // time the doors need to open or close
        private const double HoldMs = 5000;            // doors stay open this long before auto-closing
        private const double MinHoldWhenCalledMs = 1500;

        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();

        private Floor? _destination;     // floor requested but not started yet
        private Floor _movingFrom = Floor.Ground;
        private Floor _movingTo = Floor.Ground;
        private double _travel;          // 0..1 while moving
        private double _holdMs;
        private double _doorOpen;        // 0 = closed, 1 = fully open

        public LiftController()
        {
            _timer.Interval = TickMs;
            _timer.Tick += OnTick;
        }

        // ---------------- events ----------------
        public event Action Changed;                 // raised on every tick
        public event Action<string> Logged;         // text for the log table
        public event Action<bool> MovementChanged;  // true = started moving, false = stopped
        public event Action<bool> EmergencyChanged; // true = siren on

        // ---------------- state ----------------
        public Floor CurrentFloor { get; private set; } = Floor.Ground;
        public bool IsMoving { get; private set; }
        public bool IsEmergency { get; private set; }
        public DoorPhase DoorPhase { get; private set; } = DoorPhase.Closed;
        public double DoorOpenAmount { get { return _doorOpen; } }
        public Floor MovingTo { get { return _movingTo; } }

        /// <summary>Floor the lift is heading to (or has queued), or null.</summary>
        public Floor? TargetFloor
        {
            get
            {
                if (IsMoving) return _movingTo;
                return _destination;
            }
        }

        /// <summary>Lift position: 0 = Ground, 1 = First, in between while travelling.</summary>
        public double Position
        {
            get
            {
                double start = (int)_movingFrom;
                double end = (int)_movingTo;
                return IsMoving ? start + (end - start) * _travel : (int)CurrentFloor;
            }
        }

        /// <summary>Floor shown on the displays (nearest floor to the cabin).</summary>
        public Floor DisplayFloor
        {
            get { return Position >= 0.5 ? Floor.First : Floor.Ground; }
        }

        public LiftDirection Direction
        {
            get
            {
                if (!IsMoving) return LiftDirection.None;
                return _movingTo > _movingFrom ? LiftDirection.Up : LiftDirection.Down;
            }
        }

        /// <summary>Text for the big LED display.</summary>
        public string StatusText
        {
            get
            {
                if (IsEmergency)
                    return IsMoving ? "EMERGENCY - LIFT HALTED" : "EMERGENCY - SIREN ON";

                if (IsMoving)
                    return "MOVING TO " + _movingTo.DisplayName().ToUpper();

                switch (DoorPhase)
                {
                    case DoorPhase.Opening: return "DOORS OPENING";
                    case DoorPhase.Open: return "DOORS OPEN";
                    case DoorPhase.Closing: return "DOORS CLOSING";
                }

                if (_destination.HasValue)
                    return "GOING TO " + _destination.Value.DisplayName().ToUpper();

                return CurrentFloor.DisplayName().ToUpper() + " - READY";
            }
        }

        // ---------------- start / stop ----------------
        public void Start() { _timer.Start(); }

        public void Stop() { _timer.Stop(); }

        // ---------------- user actions ----------------
        /// <summary>Cabin button or call button. <paramref name="logText"/> is written to the log.</summary>
        public void RequestFloor(Floor floor, string logText)
        {
            if (IsEmergency)
            {
                Log("Request ignored - emergency mode is active");
                return;
            }

            Log(logText);

            if (IsMoving)
            {
                if (floor != _movingTo)
                    _destination = floor;      // go there after this trip
                return;
            }

            if (floor == CurrentFloor)
            {
                _destination = null;
                BeginOpening();
                return;
            }

            _destination = floor;
            BeginClosing();                    // doors must close before the lift can leave
        }

        public void PressOpen()
        {
            if (IsMoving) return;
            BeginOpening();
        }

        public void PressClose()
        {
            if (IsMoving) return;
            BeginClosing();
        }

        /// <summary>
        /// Safety sensor: something is standing in the doorway. Closing doors reopen,
        /// open doors stay open.
        /// </summary>
        public void DoorwayBlocked()
        {
            if (IsMoving) return;

            if (DoorPhase == DoorPhase.Closing)
            {
                DoorPhase = DoorPhase.Opening;
                Log("Door obstruction detected at " + CurrentFloor.DisplayName() + " - doors reopening");
            }
            else if (DoorPhase == DoorPhase.Open)
            {
                _holdMs = 0;
            }
        }

        public void ToggleEmergency()
        {
            IsEmergency = !IsEmergency;

            if (IsEmergency)
                Log(IsMoving ? "EMERGENCY button pressed - siren ON, lift halted between floors"
                             : "EMERGENCY button pressed - siren ON");
            else
                Log("Emergency cancelled - siren OFF");

            Action<bool> handler = EmergencyChanged;
            if (handler != null) handler(IsEmergency);
        }

        // ---------------- simulation ----------------
        private void OnTick(object sender, EventArgs e)
        {
            double dt = TickMs;

            if (IsMoving)
            {
                if (!IsEmergency) AdvanceTravel(dt);
            }
            else
            {
                AdvanceDoors(dt);
            }

            Action handler = Changed;
            if (handler != null) handler();
        }

        private void AdvanceTravel(double dt)
        {
            _travel += dt / TravelMs;
            if (_travel >= 1.0)
                Arrive();
        }

        private void AdvanceDoors(double dt)
        {
            switch (DoorPhase)
            {
                case DoorPhase.Closed:
                    if (IsEmergency) break;
                    if (_destination.HasValue)
                    {
                        Floor destination = _destination.Value;
                        if (destination == CurrentFloor)
                        {
                            _destination = null;
                            BeginOpening();
                        }
                        else
                        {
                            StartTravel(destination);
                        }
                    }
                    break;

                case DoorPhase.Opening:
                    _doorOpen += dt / DoorMs;
                    if (_doorOpen >= 1.0)
                    {
                        _doorOpen = 1.0;
                        DoorPhase = DoorPhase.Open;
                        _holdMs = 0;
                        Log("Doors opened at " + CurrentFloor.DisplayName());
                    }
                    break;

                case DoorPhase.Open:
                    if (IsEmergency) break;     // doors stay open during an emergency
                    _holdMs += dt;
                    if (_holdMs >= HoldMs || (_destination.HasValue && _holdMs >= MinHoldWhenCalledMs))
                        BeginClosing();
                    break;

                case DoorPhase.Closing:
                    _doorOpen -= dt / DoorMs;
                    if (_doorOpen <= 0.0)
                    {
                        _doorOpen = 0.0;
                        DoorPhase = DoorPhase.Closed;
                        Log("Doors closed at " + CurrentFloor.DisplayName());
                    }
                    break;
            }
        }

        private void BeginOpening()
        {
            if (IsMoving) return;

            if (DoorPhase == DoorPhase.Open)
                _holdMs = 0;                                   // keep doors open a bit longer
            else if (DoorPhase == DoorPhase.Closed || DoorPhase == DoorPhase.Closing)
                DoorPhase = DoorPhase.Opening;
        }

        private void BeginClosing()
        {
            if (DoorPhase == DoorPhase.Open || DoorPhase == DoorPhase.Opening)
                DoorPhase = DoorPhase.Closing;
        }

        private void StartTravel(Floor destination)
        {
            _movingFrom = CurrentFloor;
            _movingTo = destination;
            _travel = 0;
            _destination = null;
            IsMoving = true;

            Log("Lift moving " + (destination > CurrentFloor ? "up" : "down") + " to " + destination.DisplayName());

            Action<bool> handler = MovementChanged;
            if (handler != null) handler(true);
        }

        private void Arrive()
        {
            IsMoving = false;
            _travel = 0;
            CurrentFloor = _movingTo;

            if (_destination.HasValue && _destination.Value == CurrentFloor)
                _destination = null;

            Log("Lift arrived at " + CurrentFloor.DisplayName());

            Action<bool> handler = MovementChanged;
            if (handler != null) handler(false);

            BeginOpening();
        }

        private void Log(string text)
        {
            Action<string> handler = Logged;
            if (handler != null) handler(text);
        }
    }
}