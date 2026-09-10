using System;
using System.Drawing;
using System.Windows.Forms;

namespace KPassPilot
{
    /// <summary>Une carte : en-tête, contenu, barre d'actions optionnelle.</summary>
    internal sealed class Card
    {
        public Panel Root;
        public Panel Body;
        public FlowLayoutPanel Toolbar;
    }

    /// <summary>
    /// Palette et fabriques de contrôles. Tout passe par ici pour que les trois
    /// fenêtres restent homogènes et que les contrastes soient garantis.
    /// </summary>
    internal static class Theme
    {
        public static readonly Color Canvas = Color.FromArgb(243, 245, 247);
        public static readonly Color Surface = Color.FromArgb(255, 255, 255);
        public static readonly Color SurfaceAlt = Color.FromArgb(247, 249, 251);
        public static readonly Color Border = Color.FromArgb(222, 226, 231);
        public static readonly Color BorderStrong = Color.FromArgb(193, 200, 209);
        public static readonly Color Selection = Color.FromArgb(232, 240, 250);
        public static readonly Color TextPrimary = Color.FromArgb(28, 31, 35);
        public static readonly Color TextMuted = Color.FromArgb(108, 117, 127);
        public static readonly Color Accent = Color.FromArgb(30, 91, 163);
        public static readonly Color AccentHover = Color.FromArgb(23, 74, 134);
        public static readonly Color AccentSoft = Color.FromArgb(233, 240, 249);
        public static readonly Color AccentBorder = Color.FromArgb(168, 197, 228);
        public static readonly Color AccentText = Color.FromArgb(20, 73, 133);
        public static readonly Color NeutralSoft = Color.FromArgb(239, 241, 244);
        public static readonly Color NeutralBorder = Color.FromArgb(210, 215, 221);
        public static readonly Color NeutralText = Color.FromArgb(58, 64, 71);
        public static readonly Color PropSoft = Color.FromArgb(244, 239, 252);
        public static readonly Color PropBorder = Color.FromArgb(207, 191, 236);
        public static readonly Color PropText = Color.FromArgb(87, 55, 149);
        public static readonly Color Danger = Color.FromArgb(176, 42, 47);
        public static readonly Color DangerHover = Color.FromArgb(250, 238, 238);
        public static readonly Color OkBg = Color.FromArgb(238, 247, 241);
        public static readonly Color OkBorder = Color.FromArgb(183, 219, 197);
        public static readonly Color OkText = Color.FromArgb(21, 98, 60);
        public static readonly Color WarnBg = Color.FromArgb(255, 248, 233);
        public static readonly Color WarnBorder = Color.FromArgb(238, 214, 156);
        public static readonly Color WarnText = Color.FromArgb(138, 90, 8);
        public static readonly Color ErrBg = Color.FromArgb(253, 240, 240);
        public static readonly Color ErrBorder = Color.FromArgb(235, 190, 190);
        public static readonly Color ErrText = Color.FromArgb(158, 38, 43);
        public static readonly Font Body = new Font("Segoe UI", 9f);
        public static readonly Font BodyBold = new Font("Segoe UI", 9f, FontStyle.Bold);
        public static readonly Font Small = new Font("Segoe UI", 8.25f);
        public static readonly Font SmallBold = new Font("Segoe UI", 8.25f, FontStyle.Bold);
        public static readonly Font Section = new Font("Segoe UI", 9.75f, FontStyle.Bold);
        public static readonly Font Title = new Font("Segoe UI", 13.5f);
        public static readonly Font Input = new Font("Segoe UI", 10f);
        public static readonly Font Mono = new Font("Consolas", 10f);
        public static readonly Font MonoSmall = new Font("Consolas", 8.75f);

        public static Color TextOn(Color bg)
        {
            double l = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255.0;
            return l > 0.62 ? TextPrimary : Color.White;
        }

        public static Color FromHex(string hex)
        {
            try
            {
                hex = (hex ?? "").Trim().TrimStart('#');
                if (hex.Length != 6) return Accent;
                return Color.FromArgb(Convert.ToInt32(hex.Substring(0, 2), 16), Convert.ToInt32(hex.Substring(2, 2), 16), Convert.ToInt32(hex.Substring(4, 2), 16));
            }
            catch { return Accent; }
        }

        public static string ToHex(Color c) { return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2"); }

        private static Button BaseButton(string text)
        {
            var b = new Button { Text = text, FlatStyle = FlatStyle.Flat, Font = Body, Height = 30, AutoSize = false, UseVisualStyleBackColor = false, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter, Margin = new Padding(0, 0, 6, 0) };
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        public static Button Primary(string text, int width)
        {
            var b = BaseButton(text); b.Width = width; b.Height = 34; b.Font = BodyBold; b.FlatAppearance.MouseOverBackColor = AccentHover; Paint(b, Accent, Color.White, Accent); return b;
        }

        private static void Paint(Button b, Color back, Color fore, Color border)
        {
            EventHandler apply = (s, e) =>
            {
                if (b.Enabled) { b.BackColor = back; b.ForeColor = fore; b.FlatAppearance.BorderColor = border; }
                else { b.BackColor = NeutralSoft; b.ForeColor = Color.FromArgb(150, 156, 163); b.FlatAppearance.BorderColor = NeutralBorder; }
            };
            b.EnabledChanged += apply; apply(b, EventArgs.Empty);
        }

        public static Button Secondary(string text, int width) { var b = BaseButton(text); b.Width = width; b.FlatAppearance.MouseOverBackColor = SurfaceAlt; Paint(b, Surface, TextPrimary, BorderStrong); return b; }
        public static Button Quiet(string text, int width) { var b = BaseButton(text); b.Width = width; b.FlatAppearance.MouseOverBackColor = NeutralSoft; Paint(b, SurfaceAlt, TextMuted, Border); return b; }
        public static Button Destructive(string text, int width) { var b = BaseButton(text); b.Width = width; b.FlatAppearance.MouseOverBackColor = DangerHover; Paint(b, Surface, Danger, Color.FromArgb(228, 190, 191)); return b; }

        public static Label SectionTitle(string text) { return new Label { Text = text, Font = Section, ForeColor = TextPrimary, AutoSize = false, Height = 20, TextAlign = ContentAlignment.MiddleLeft }; }
        public static Label Hint(string text) { return new Label { Text = text, Font = Small, ForeColor = TextMuted, AutoSize = false, Height = 16, TextAlign = ContentAlignment.MiddleLeft }; }
        public static Label FieldCaption(string text) { return new Label { Text = text, Font = BodyBold, ForeColor = TextPrimary, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }; }
        public static TextBox Input1(int width) { return new TextBox { Width = width, Font = Input, BorderStyle = BorderStyle.FixedSingle }; }

        public static void SetStatus(Label lbl, string text, int level)
        {
            lbl.Text = text;
            if (level == 1) { lbl.BackColor = WarnBg; lbl.ForeColor = WarnText; }
            else if (level == 2) { lbl.BackColor = ErrBg; lbl.ForeColor = ErrText; }
            else if (level == 3) { lbl.BackColor = OkBg; lbl.ForeColor = OkText; }
            else { lbl.BackColor = SurfaceAlt; lbl.ForeColor = TextMuted; }
        }

        public static Card MakeCard(string title, bool withToolbar)
        {
            var card = new Card();
            card.Root = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(1), Margin = new Padding(5) };
            var root = card.Root;
            root.Paint += (s, e) => { using (var p = new Pen(Border)) e.Graphics.DrawRectangle(p, 0, 0, root.Width - 1, root.Height - 1); };
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Surface, Margin = new Padding(0) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var head = new Panel { Dock = DockStyle.Fill, BackColor = SurfaceAlt, Margin = new Padding(0) };
            head.Paint += (s, e) => { using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, head.Height - 1, head.Width, head.Height - 1); };
            head.Controls.Add(new Label { Dock = DockStyle.Fill, Text = title, Font = Section, ForeColor = TextPrimary, Padding = new Padding(11, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft });
            grid.Controls.Add(head, 0, 0);
            card.Body = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(0) };
            grid.Controls.Add(card.Body, 0, 1);
            card.Toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = SurfaceAlt, Padding = new Padding(7, 6, 7, 6), Margin = new Padding(0), WrapContents = true, Visible = withToolbar };
            if (withToolbar) card.Toolbar.Paint += (s, e) => { using (var p = new Pen(Border)) e.Graphics.DrawLine(p, 0, 0, card.Toolbar.Width, 0); };
            grid.Controls.Add(card.Toolbar, 0, 2);
            root.Controls.Add(grid);
            return card;
        }

        public static Panel PageHeader(string title, string subtitle)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(0) };
            p.Paint += (s, e) => { using (var pen = new Pen(Border)) e.Graphics.DrawLine(pen, 0, p.Height - 1, p.Width, p.Height - 1); using (var b = new SolidBrush(Accent)) e.Graphics.FillRectangle(b, 0, 0, 4, p.Height - 1); };
            var sub = new Label { Text = subtitle, Font = Small, ForeColor = TextMuted, Left = 20, Top = 33, Height = 18, AutoSize = false, AutoEllipsis = true, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            var head = new Label { Text = title, Font = Title, ForeColor = TextPrimary, Left = 18, Top = 10, Height = 26, AutoSize = false, AutoEllipsis = true, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            p.Resize += (s, e) => { sub.Width = Math.Max(80, p.Width - 34); head.Width = Math.Max(80, p.Width - 32); };
            p.Controls.Add(sub); p.Controls.Add(head); return p;
        }

        public static Panel PageFooter()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = SurfaceAlt, Margin = new Padding(0) };
            p.Paint += (s, e) => { using (var pen = new Pen(Border)) e.Graphics.DrawLine(pen, 0, 0, p.Width, 0); };
            return p;
        }

        public static TableLayoutPanel PageGrid()
        {
            var g = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Canvas, Margin = new Padding(0), Padding = new Padding(0) };
            g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            g.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
            g.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            g.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            return g;
        }
    }
}
