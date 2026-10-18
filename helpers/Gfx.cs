using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ElevatorApp
{
    /// <summary>Small drawing helpers shared by the custom controls.</summary>
    public static class Gfx
    {
        /// <summary>Counter that increases every <paramref name="ms"/> milliseconds (used for blinking/animation).</summary>
        public static int Ticks(int ms)
        {
            return (int)((Environment.TickCount & 0x7FFFFFFF) / ms);
        }

        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radius * 2f, Math.Min(r.Width, r.Height));
            if (d <= 1f)
            {
                path.AddRectangle(r);
                return path;
            }

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Brushed-steel gradient (light and dark bands).</summary>
        public static LinearGradientBrush MetalBrush(RectangleF r, LinearGradientMode mode)
        {
            if (r.Width < 1f) r.Width = 1f;
            if (r.Height < 1f) r.Height = 1f;

            var brush = new LinearGradientBrush(r, Color.Gray, Color.White, mode);
            var blend = new ColorBlend(5);
            blend.Positions = new float[] { 0f, 0.22f, 0.5f, 0.78f, 1f };
            blend.Colors = new Color[]
            {
                Color.FromArgb(120, 120, 126),
                Color.FromArgb(206, 206, 211),
                Color.FromArgb(160, 160, 166),
                Color.FromArgb(226, 226, 231),
                Color.FromArgb(132, 132, 138)
            };
            brush.InterpolationColors = blend;
            return brush;
        }

        public static void DrawCentered(Graphics g, string text, Font font, Brush brush, RectangleF rect, bool noWrap = false)
        {
            using (var format = new StringFormat())
            {
                if (noWrap) format.FormatFlags = StringFormatFlags.NoWrap;
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(text, font, brush, rect, format);
            }
        }

        /// <summary>Returns a copy of the rectangle moved by (dx, dy).</summary>
        public static RectangleF Offset(RectangleF r, float dx, float dy)
        {
            return new RectangleF(r.X + dx, r.Y + dy, r.Width, r.Height);
        }

        public static Color Lighten(Color c, int amount)
        {
            return Color.FromArgb(
                c.A,
                Math.Min(255, c.R + amount),
                Math.Min(255, c.G + amount),
                Math.Min(255, c.B + amount));
        }
    }
}
