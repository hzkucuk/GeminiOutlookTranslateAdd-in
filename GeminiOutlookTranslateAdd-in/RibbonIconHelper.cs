using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace GeminiOutlookTranslateAdd_in
{
    internal static class RibbonIconHelper
    {
        private static readonly Color TurkishRed = Color.FromArgb(229, 57, 53);
        private static readonly Color EnglishBlue = Color.FromArgb(21, 101, 192);
        private static readonly Color StopRed = Color.FromArgb(198, 40, 40);
        private static readonly Color ArrowGray = Color.FromArgb(117, 117, 117);

        public static Bitmap CreateTrToEnIcon()
        {
            return CreateTranslateIcon("TR", "EN", TurkishRed, EnglishBlue);
        }

        public static Bitmap CreateEnToTrIcon()
        {
            return CreateTranslateIcon("EN", "TR", EnglishBlue, TurkishRed);
        }

        public static Bitmap CreateStopIcon()
        {
            var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (var brush = new SolidBrush(StopRed))
                {
                    g.FillEllipse(brush, 2, 2, 28, 28);
                }

                using (var pen = new Pen(Color.White, 3f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, 10, 10, 22, 22);
                    g.DrawLine(pen, 22, 10, 10, 22);
                }
            }
            return bmp;
        }

        private static Bitmap CreateTranslateIcon(string fromLang, string toLang, Color fromColor, Color toColor)
        {
            var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.Clear(Color.Transparent);

                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (var path = CreateRoundedRect(new Rectangle(2, 1, 28, 12), 4))
                using (var brush = new SolidBrush(fromColor))
                {
                    g.FillPath(brush, path);
                }
                using (var font = new Font("Segoe UI", 6.5f, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(fromLang, font, brush, new RectangleF(2, 1, 28, 12), sf);
                }

                using (var pen = new Pen(ArrowGray, 2f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, 16, 14, 16, 18);
                    g.DrawLine(pen, 12, 16, 16, 19);
                    g.DrawLine(pen, 20, 16, 16, 19);
                }

                using (var path = CreateRoundedRect(new Rectangle(2, 19, 28, 12), 4))
                using (var brush = new SolidBrush(toColor))
                {
                    g.FillPath(brush, path);
                }
                using (var font = new Font("Segoe UI", 6.5f, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(toLang, font, brush, new RectangleF(2, 19, 28, 12), sf);
                }
            }
            return bmp;
        }

        private static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
