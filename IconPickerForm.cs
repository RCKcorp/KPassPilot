using System;
using System.Drawing;
using System.Windows.Forms;

namespace KPassPilot
{
    public class IconPickerForm : Form
    {
        private readonly FlowLayoutPanel grid;
        private readonly Label lblPick;
        public int SelectedIcon { get; private set; }

        public IconPickerForm(int current, ImageList icons)
        {
            SelectedIcon = current;
            Text = "Choisir une icône"; Width = 520; Height = 480; MinimumSize = new Size(460, 420);
            StartPosition = FormStartPosition.CenterParent; MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
            BackColor = Theme.Canvas; Font = Theme.Body;

            var page = Theme.PageGrid(); Controls.Add(page);
            page.Controls.Add(Theme.PageHeader("Icône de l'entrée", "Celle qui s'affichera dans l'arborescence KeePass."), 0, 0);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Canvas, Padding = new Padding(6, 6, 6, 2), Margin = new Padding(0) };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
            page.Controls.Add(content, 0, 1);

            var card = Theme.MakeCard("Icônes disponibles", false);
            grid = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true, FlowDirection = FlowDirection.LeftToRight, BackColor = Theme.Surface, Padding = new Padding(10) };
            int count = icons != null ? Math.Min(69, icons.Images.Count) : 69;
            for (int i = 0; i < count; i++) grid.Controls.Add(MakeTile(i, icons));
            card.Body.Controls.Add(grid); content.Controls.Add(card.Root, 0, 0);

            lblPick = new Label { Dock = DockStyle.Fill, Font = Theme.Small, ForeColor = Theme.TextMuted, BackColor = Theme.SurfaceAlt, Padding = new Padding(11, 0, 11, 0), TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(5, 0, 5, 4) };
            content.Controls.Add(lblPick, 0, 1);
            page.Controls.Add(BuildFooter(), 0, 2); RefreshPick();
            Shown += (s, e) => { if (SelectedIcon >= 0 && SelectedIcon < grid.Controls.Count) grid.ScrollControlIntoView(grid.Controls[SelectedIcon]); };
        }

        private Control BuildFooter()
        {
            var footer = Theme.PageFooter();
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.SurfaceAlt, Padding = new Padding(14, 11, 16, 0), Margin = new Padding(0) };
            var ok = Theme.Primary("Choisir", 110); ok.Click += (s, e) => Accept(); flow.Controls.Add(ok);
            var cancel = Theme.Secondary("Annuler", 100); cancel.DialogResult = DialogResult.Cancel; flow.Controls.Add(cancel);
            footer.Controls.Add(flow); AcceptButton = ok; CancelButton = cancel; return footer;
        }

        private Control MakeTile(int id, ImageList icons)
        {
            Image img = icons != null && id < icons.Images.Count ? icons.Images[id] : null;
            var tile = new Panel { Width = 38, Height = 38, Margin = new Padding(0, 0, 6, 6), Cursor = Cursors.Hand, Tag = id };
            tile.Paint += (s, e) =>
            {
                bool sel = SelectedIcon == id;
                using (var b = new SolidBrush(sel ? Theme.AccentSoft : Theme.Surface)) e.Graphics.FillRectangle(b, 0, 0, tile.Width, tile.Height);
                using (var p = new Pen(sel ? Theme.Accent : Theme.Border)) e.Graphics.DrawRectangle(p, 0, 0, tile.Width - 1, tile.Height - 1);
                if (img != null) e.Graphics.DrawImage(img, (tile.Width - img.Width) / 2, (tile.Height - img.Height) / 2, img.Width, img.Height);
                else using (var b = new SolidBrush(sel ? Theme.AccentText : Theme.TextMuted)) using (var f = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) e.Graphics.DrawString(id.ToString(), Theme.Small, b, new RectangleF(0, 0, tile.Width, tile.Height), f);
            };
            tile.Click += (s, e) => Select(id); tile.DoubleClick += (s, e) => { Select(id); Accept(); };
            new ToolTip().SetToolTip(tile, "Icône n° " + id); return tile;
        }

        private void Select(int id) { SelectedIcon = id; foreach (Control c in grid.Controls) c.Invalidate(); RefreshPick(); }
        private void RefreshPick() { lblPick.Text = "Icône n° " + SelectedIcon + " sélectionnée."; }
        private void Accept() { DialogResult = DialogResult.OK; Close(); }
    }
}
