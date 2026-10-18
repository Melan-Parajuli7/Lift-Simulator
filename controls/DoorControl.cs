using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace ElevatorApp
{
    /// <summary>
    /// One floor's lift door: indicator light + floor display on top, a steel frame with two
    /// sliding doors, and the cabin interior that becomes visible when the doors open.
    /// </summary>
    public class DoorControl : Control
    {
        private double _openAmount;
        private bool _liftHere;
        private bool _liftArriving;
        private string _indicatorText = "G";
        private LiftDirection _direction = LiftDirection.None;
        private string _caption = "";

        private const float IndicatorHeightRatio = 0.115f;
        private const float CaptionHeight = 26f;

        public DoorControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Size = new Size(230, 350);
        }

        /// <summary>0 = doors closed, 1 = fully open.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double OpenAmount
        {
            get { return _openAmount; }
            set
            {
                value = Math.Max(0.0, Math.Min(1.0, value));
                if (Math.Abs(_openAmount - value) > 0.0005) { _openAmount = value; Invalidate(); }
            }
        }

        /// <summary>True when the cabin is standing at this floor (green light).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LiftHere
        {
            get { return _liftHere; }
            set { if (_liftHere != value) { _liftHere = value; Invalidate(); } }
        }

        /// <summary>True while the cabin is travelling to this floor (amber blinking light).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LiftArriving
        {
            get { return _liftArriving; }
            set { if (_liftArriving != value) { _liftArriving = value; Invalidate(); } }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IndicatorText
        {
            get { return _indicatorText; }
            set { if (_indicatorText != value) { _indicatorText = value; Invalidate(); } }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LiftDirection Direction
        {
            get { return _direction; }
            set { if (_direction != value) { _direction = value; Invalidate(); } }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Caption
        {
            get { return _caption; }
            set { if (_caption != value) { _caption = value; Invalidate(); } }
        }

        /// <summary>True while the mouse cursor is inside the doorway (acts like the safety sensor of a real lift).</summary>
        public bool IsCursorInDoorway
        {
            get
            {
                if (!IsHandleCreated) return false;
                Point p = PointToClient(Control.MousePosition);
                return GetOpeningRect(GetFrameRect()).Contains(p.X, p.Y);
            }
        }

        private RectangleF GetFrameRect()
        {
            float frameTop = Height * IndicatorHeightRatio + 12;
            return new RectangleF(4, frameTop, Width - 8, Height - frameTop - CaptionHeight);
        }

        private static RectangleF GetOpeningRect(RectangleF frame)
        {
            float side = frame.Width * 0.085f;
            float top = side * 1.4f;
            return new RectangleF(frame.X + side, frame.Y + top, frame.Width - 2 * side, frame.Height - top - side * 0.45f);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            float indicatorHeight = Height * IndicatorHeightRatio;

            DrawIndicator(g, new RectangleF(Width * 0.18f, 2, Width * 0.64f, indicatorHeight));
            DrawDoorFrame(g, GetFrameRect());

            using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(95, 95, 100)))
                Gfx.DrawCentered(g, _caption, font, brush, new RectangleF(0, Height - CaptionHeight, Width, CaptionHeight));
        }

        // ------------------------------------------------------------------ indicator
        private void DrawIndicator(Graphics g, RectangleF rect)
        {
            using (GraphicsPath path = Gfx.RoundedRect(rect, 5))
            {
                using (var brush = Gfx.MetalBrush(rect, LinearGradientMode.Vertical))
                    g.FillPath(brush, path);
                using (var pen = new Pen(Color.FromArgb(80, 80, 86), 1.2f))
                    g.DrawPath(pen, path);
            }

            RectangleF inner = RectangleF.Inflate(rect, -3.5f, -3.5f);
            using (GraphicsPath path = Gfx.RoundedRect(inner, 3))
            using (var brush = new SolidBrush(Color.Black))
                g.FillPath(brush, path);

            // lamp: green = lift here, amber blinking = lift arriving, red = lift not here
            float lampSize = inner.Height * 0.66f;
            var lamp = new RectangleF(inner.X + 9, inner.Y + (inner.Height - lampSize) / 2f, lampSize, lampSize);
            DrawLamp(g, lamp);

            // floor label (+ arrow while moving)
            var textArea = new RectangleF(lamp.Right + 6, inner.Y, inner.Right - lamp.Right - 10, inner.Height);
            using (var font = new Font("Consolas", inner.Height * 0.66f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color.FromArgb(255, 45, 45)))
            {
                if (_direction == LiftDirection.None)
                {
                    Gfx.DrawCentered(g, _indicatorText, font, brush, textArea);
                }
                else
                {
                    float arrowW = inner.Height * 0.36f;
                    float arrowH = inner.Height * 0.40f;
                    float arrowX = textArea.X + textArea.Width * 0.28f;
                    float arrowY = inner.Y + inner.Height / 2f;

                    PointF[] arrow = _direction == LiftDirection.Up
                        ? new PointF[] { new PointF(arrowX - arrowW, arrowY + arrowH / 2), new PointF(arrowX + arrowW, arrowY + arrowH / 2), new PointF(arrowX, arrowY - arrowH / 2) }
                        : new PointF[] { new PointF(arrowX - arrowW, arrowY - arrowH / 2), new PointF(arrowX + arrowW, arrowY - arrowH / 2), new PointF(arrowX, arrowY + arrowH / 2) };
                    g.FillPolygon(brush, arrow);

                    var labelArea = new RectangleF(textArea.X + textArea.Width * 0.5f, textArea.Y, textArea.Width * 0.5f, textArea.Height);
                    Gfx.DrawCentered(g, _indicatorText, font, brush, labelArea);
                }
            }
        }

        private void DrawLamp(Graphics g, RectangleF lamp)
        {
            Color core, edge;
            bool glowing;

            if (_liftHere)
            {
                core = Color.FromArgb(170, 255, 170);
                edge = Color.FromArgb(0, 150, 0);
                glowing = true;
            }
            else if (_liftArriving)
            {
                bool on = Gfx.Ticks(350) % 2 == 0;
                core = on ? Color.FromArgb(255, 225, 100) : Color.FromArgb(110, 80, 0);
                edge = on ? Color.FromArgb(225, 140, 0) : Color.FromArgb(50, 35, 0);
                glowing = on;
            }
            else
            {
                core = Color.FromArgb(255, 120, 120);
                edge = Color.FromArgb(150, 0, 0);
                glowing = false;
            }

            if (glowing)
            {
                RectangleF halo = RectangleF.Inflate(lamp, 4, 4);
                using (var brush = new SolidBrush(Color.FromArgb(70, core)))
                    g.FillEllipse(brush, halo);
            }

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(lamp);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = core;
                    brush.SurroundColors = new Color[] { edge };
                    brush.CenterPoint = new PointF(lamp.X + lamp.Width * 0.4f, lamp.Y + lamp.Height * 0.35f);
                    g.FillPath(brush, path);
                }
            }

            using (var pen = new Pen(Color.FromArgb(120, 20, 20, 20), 1f))
                g.DrawEllipse(pen, lamp);

            var spot = new RectangleF(lamp.X + lamp.Width * 0.22f, lamp.Y + lamp.Height * 0.14f, lamp.Width * 0.32f, lamp.Height * 0.24f);
            using (var brush = new SolidBrush(Color.FromArgb(150, 255, 255, 255)))
                g.FillEllipse(brush, spot);
        }

        // ------------------------------------------------------------------ door frame
        private void DrawDoorFrame(Graphics g, RectangleF frame)
        {
            using (GraphicsPath path = Gfx.RoundedRect(frame, 6))
            {
                using (var brush = Gfx.MetalBrush(frame, LinearGradientMode.Horizontal))
                    g.FillPath(brush, path);
                using (var pen = new Pen(Color.FromArgb(88, 88, 94), 1.5f))
                    g.DrawPath(pen, path);
            }

            RectangleF opening = GetOpeningRect(frame);

            // lighter header band
            var header = new RectangleF(frame.X, frame.Y, frame.Width, opening.Y - frame.Y);
            using (var brush = new LinearGradientBrush(header, Color.FromArgb(120, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
                g.FillRectangle(brush, header);

            GraphicsState state = g.Save();
            g.SetClip(opening);

            if (_liftHere && _openAmount > 0.001)
                DrawCabin(g, opening);
            else
                DrawShaft(g, opening);

            DrawPanels(g, opening);

            // shadow under the header
            var shade = new RectangleF(opening.X, opening.Y, opening.Width, opening.Height * 0.07f);
            using (var brush = new LinearGradientBrush(shade, Color.FromArgb(110, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), 90f))
                g.FillRectangle(brush, shade);

            g.Restore(state);

            using (var pen = new Pen(Color.FromArgb(55, 55, 60), 2f))
                g.DrawRectangle(pen, opening.X, opening.Y, opening.Width, opening.Height);
        }

        private static void DrawShaft(Graphics g, RectangleF rect)
        {
            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(34, 34, 38), Color.FromArgb(10, 10, 12), 90f))
                g.FillRectangle(brush, rect);
        }

        private static void DrawCabin(Graphics g, RectangleF rect)
        {
            // back wall
            using (var brush = new LinearGradientBrush(rect, Color.Gray, Color.White, LinearGradientMode.Horizontal))
            {
                var blend = new ColorBlend(3);
                blend.Positions = new float[] { 0f, 0.5f, 1f };
                blend.Colors = new Color[] { Color.FromArgb(76, 76, 82), Color.FromArgb(188, 188, 194), Color.FromArgb(76, 76, 82) };
                brush.InterpolationColors = blend;
                g.FillRectangle(brush, rect);
            }

            // wall seams
            using (var pen = new Pen(Color.FromArgb(55, 0, 0, 0), 1f))
            {
                g.DrawLine(pen, rect.X + rect.Width / 3f, rect.Y, rect.X + rect.Width / 3f, rect.Bottom);
                g.DrawLine(pen, rect.X + rect.Width * 2f / 3f, rect.Y, rect.X + rect.Width * 2f / 3f, rect.Bottom);
            }

            // ceiling with spot lights
            var ceiling = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height * 0.16f);
            using (var brush = new LinearGradientBrush(ceiling, Color.FromArgb(22, 22, 25), Color.FromArgb(62, 62, 68), 90f))
                g.FillRectangle(brush, ceiling);

            for (int i = 0; i < 3; i++)
            {
                float lx = rect.X + rect.Width * (0.25f + 0.25f * i);
                float ly = ceiling.Y + ceiling.Height * 0.55f;
                var lightGlow = new RectangleF(lx - rect.Width * 0.07f, ly - ceiling.Height * 0.32f, rect.Width * 0.14f, ceiling.Height * 0.64f);
                var lightCore = new RectangleF(lx - rect.Width * 0.035f, ly - ceiling.Height * 0.14f, rect.Width * 0.07f, ceiling.Height * 0.28f);
                using (var brush = new SolidBrush(Color.FromArgb(70, 255, 255, 235))) g.FillEllipse(brush, lightGlow);
                using (var brush = new SolidBrush(Color.FromArgb(250, 255, 255, 245))) g.FillEllipse(brush, lightCore);
            }

            // hand rail
            var rail = new RectangleF(rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.58f, rect.Width * 0.90f, 6f);
            using (var brush = Gfx.MetalBrush(rail, LinearGradientMode.Vertical))
                g.FillRectangle(brush, rail);

            // floor
            var floor = new RectangleF(rect.X, rect.Bottom - rect.Height * 0.13f, rect.Width, rect.Height * 0.13f);
            using (var brush = new LinearGradientBrush(floor, Color.FromArgb(205, 205, 210), Color.FromArgb(92, 92, 98), 90f))
                g.FillRectangle(brush, floor);
        }

        private void DrawPanels(Graphics g, RectangleF opening)
        {
            float panelWidth = opening.Width / 2f;
            float gap = (float)_openAmount * panelWidth * 0.80f;   // panels stay partly visible at the sides

            DrawPanel(g, new RectangleF(opening.X - gap, opening.Y, panelWidth + 0.5f, opening.Height), true);
            DrawPanel(g, new RectangleF(opening.X + panelWidth + gap - 0.5f, opening.Y, panelWidth + 0.5f, opening.Height), false);
        }

        private static void DrawPanel(Graphics g, RectangleF rect, bool isLeft)
        {
            using (var brush = Gfx.MetalBrush(rect, LinearGradientMode.Horizontal))
                g.FillRectangle(brush, rect);

            // soft vertical sheen
            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(70, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
                g.FillRectangle(brush, rect);

            // darker bottom
            var bottom = new RectangleF(rect.X, rect.Y + rect.Height * 0.7f, rect.Width, rect.Height * 0.3f);
            using (var brush = new LinearGradientBrush(bottom, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(55, 0, 0, 0), 90f))
                g.FillRectangle(brush, bottom);

            // brushed lines
            using (var pen = new Pen(Color.FromArgb(22, 0, 0, 0), 1f))
            {
                for (float x = rect.X + 5; x < rect.Right; x += 6)
                    g.DrawLine(pen, x, rect.Y, x, rect.Bottom);
            }

            // centre edge of each door
            float edge = isLeft ? rect.Right - 1 : rect.X + 1;
            using (var pen = new Pen(Color.FromArgb(130, 45, 45, 50), 2f))
                g.DrawLine(pen, edge, rect.Y, edge, rect.Bottom);
        }
    }
}