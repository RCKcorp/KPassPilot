using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KPassPilot
{
    internal static class Dialogs
    {
        public static string Prompt(IWin32Window owner, string title, string message, string initial)
        {
            using (var f = NewShell(title, 470))
            {
                var body = (TableLayoutPanel)f.Tag;
                body.Controls.Add(Message(message), 0, 0);
                var box = new TextBox { Text = initial ?? "", Font = Theme.Input, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Top, Margin = new Padding(0, 10, 0, 0) };
                box.SelectAll();
                var host = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface, Margin = new Padding(0) };
                host.Controls.Add(box);
                body.Controls.Add(host, 0, 1);
                var ok = Theme.Primary("Valider", 110);
                var cancel = Theme.Secondary("Annuler", 100);
                Buttons(f, ok, cancel);
                f.ActiveControl = box;
                return f.ShowDialog(owner) == DialogResult.OK ? box.Text : null;
            }
        }

        public static bool Confirm(IWin32Window owner, string title, string message)
        {
            using (var f = NewShell(title, 470))
            {
                var body = (TableLayoutPanel)f.Tag;
                body.Controls.Add(Message(title), 0, 0);
                if (!string.IsNullOrEmpty(message)) body.Controls.Add(Detail(message), 0, 1);
                var ok = Theme.Destructive("Confirmer", 120);
                var cancel = Theme.Secondary("Annuler", 100);
                Buttons(f, ok, cancel);
                f.AcceptButton = cancel;
                return f.ShowDialog(owner) == DialogResult.OK;
            }
        }

        public static void Info(IWin32Window owner, string title, string message)
        {
            using (var f = NewShell(title, 470))
            {
                var body = (TableLayoutPanel)f.Tag;
                body.Controls.Add(Message(title), 0, 0);
                if (!string.IsNullOrEmpty(message)) body.Controls.Add(Detail(message), 0, 1);
                var ok = Theme.Primary("Fermer", 110);
                Buttons(f, ok, null);
                f.ShowDialog(owner);
            }
        }

        public static int Pick(IWin32Window owner, string title, string message, List<string> items)
        {
            using (var f = NewShell(title, 470))
            {
                f.Height = 400;
                var body = (TableLayoutPanel)f.Tag;
                body.Controls.Add(Message(message), 0, 0);
                var list = new ListBox { Dock = DockStyle.Fill, Font = Theme.Body, BorderStyle = BorderStyle.FixedSingle, IntegralHeight = false, Margin = new Padding(0, 10, 0, 0) };
                if (items != null) foreach (var s in items) list.Items.Add(s);
                if (list.Items.Count > 0) list.SelectedIndex = 0;
                body.Controls.Add(list, 0, 1);
                var ok = Theme.Primary("Choisir", 110);
                var cancel = Theme.Secondary("Annuler", 100);
                Buttons(f, ok, cancel);
                list.DoubleClick += (s, e) => { if (list.SelectedIndex >= 0) { f.DialogResult = DialogResult.OK; f.Close(); } };
                f.ActiveControl = list;
                return f.ShowDialog(owner) == DialogResult.OK ? list.SelectedIndex : -1;
            }
        }

        private static Form NewShell(string title, int width)
        {
            var f = new Form { Text = title, Width = width, Height = 230, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false, BackColor = Theme.Surface, Font = Theme.Body };
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Surface, Margin = new Padding(0) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            f.Controls.Add(grid);
            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Surface, Padding = new Padding(20, 18, 20, 8), Margin = new Padding(0) };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            grid.Controls.Add(body, 0, 0);
            var footer = Theme.PageFooter();
            grid.Controls.Add(footer, 0, 1);
            f.Tag = body;
            return f;
        }

        private static Label Message(string text) { return new Label { Text = text ?? "", Font = Theme.BodyBold, ForeColor = Theme.TextPrimary, AutoSize = true, MaximumSize = new Size(400, 0), Margin = new Padding(0) }; }
        private static Label Detail(string text) { return new Label { Text = text ?? "", Font = Theme.Body, ForeColor = Theme.TextMuted, AutoSize = true, MaximumSize = new Size(400, 0), Margin = new Padding(0, 8, 0, 0) }; }

        private static void Buttons(Form f, Button ok, Button cancel)
        {
            var grid = (TableLayoutPanel)f.Controls[0];
            var footer = (Panel)grid.GetControlFromPosition(0, 1);
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.SurfaceAlt, Padding = new Padding(16, 11, 18, 0), Margin = new Padding(0) };
            ok.DialogResult = DialogResult.OK; ok.Height = 34; flow.Controls.Add(ok); f.AcceptButton = ok;
            if (cancel != null) { cancel.DialogResult = DialogResult.Cancel; cancel.Height = 34; flow.Controls.Add(cancel); f.CancelButton = cancel; }
            footer.Controls.Add(flow);
        }
    }
}
