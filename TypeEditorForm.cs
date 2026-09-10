using System;
using System.Drawing;
using System.Windows.Forms;

namespace KPassPilot
{
    public class TypeEditorForm : Form
    {
        private readonly TextBox txtName;
        private readonly TextBox txtDesc;
        private readonly Button btnIcon;
        private readonly ImageList keepassIcons;
        private int selectedIconId;

        public string TypeName { get; private set; }
        public string Description { get; private set; }
        public int IconId { get; private set; }

        public TypeEditorForm(string initialName = "", string initialDesc = "", int initialIconId = 0, ImageList icons = null)
        {
            selectedIconId = initialIconId;
            keepassIcons = icons;
            TypeName = initialName;
            Description = initialDesc;

            Text = string.IsNullOrEmpty(initialName) ? "Nouveau profil" : "Modifier le profil";
            Width = 520; Height = 380; MinimumSize = new Size(480, 360);
            StartPosition = FormStartPosition.CenterParent; MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
            BackColor = Theme.Canvas; Font = Theme.Body;

            var page = Theme.PageGrid(); Controls.Add(page);
            page.Controls.Add(Theme.PageHeader(string.IsNullOrEmpty(initialName) ? "Nouveau profil" : "Modifier", "Nom, description et icône du profil."), 0, 0);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, BackColor = Theme.Canvas, Padding = new Padding(20, 14, 20, 0), Margin = new Padding(0) };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            page.Controls.Add(content, 0, 1);

            content.Controls.Add(Label("Nom"), 0, 0);
            txtName = new TextBox { Text = initialName, Font = Theme.Input, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12) };
            content.Controls.Add(txtName, 0, 1);
            content.Controls.Add(Label("Description"), 0, 2);
            txtDesc = new TextBox { Text = initialDesc, Font = Theme.Input, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12) };
            content.Controls.Add(txtDesc, 0, 3);
            content.Controls.Add(Label("Icône"), 0, 4);

            var iconRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = false, BackColor = Theme.Canvas, Margin = new Padding(0) };
            btnIcon = Theme.Secondary("", 50); btnIcon.Height = 36; btnIcon.ImageAlign = ContentAlignment.MiddleCenter; btnIcon.Margin = new Padding(0); btnIcon.Click += (s, e) => PickIcon();
            UpdateIconButton(); iconRow.Controls.Add(btnIcon);
            var pickBtn = Theme.Secondary("Choisir", 100); pickBtn.Height = 34; pickBtn.Margin = new Padding(8, 2, 0, 0); pickBtn.Click += (s, e) => PickIcon(); iconRow.Controls.Add(pickBtn);
            content.Controls.Add(iconRow, 0, 5);
            page.Controls.Add(BuildFooter(), 0, 2);
        }

        private Label Label(string text) { return new Label { Text = text, Font = Theme.BodyBold, ForeColor = Theme.TextPrimary, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 0, 0, 2) }; }

        private Control BuildFooter()
        {
            var footer = Theme.PageFooter();
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.SurfaceAlt, Padding = new Padding(14, 11, 16, 0), Margin = new Padding(0) };
            var ok = Theme.Primary("Valider", 110); ok.Click += (s, e) => Accept(); flow.Controls.Add(ok);
            var cancel = Theme.Secondary("Annuler", 100); cancel.DialogResult = DialogResult.Cancel; flow.Controls.Add(cancel);
            footer.Controls.Add(flow); AcceptButton = ok; CancelButton = cancel; return footer;
        }

        private void PickIcon()
        {
            using (var form = new IconPickerForm(selectedIconId, keepassIcons))
                if (form.ShowDialog(this) == DialogResult.OK) { selectedIconId = form.SelectedIcon; UpdateIconButton(); }
        }

        private void UpdateIconButton()
        {
            if (keepassIcons != null && selectedIconId >= 0 && selectedIconId < keepassIcons.Images.Count) { btnIcon.Image = keepassIcons.Images[selectedIconId]; btnIcon.Text = ""; }
            else { btnIcon.Text = "Icon\n" + selectedIconId; btnIcon.Image = null; }
        }

        private void Accept()
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { Dialogs.Info(this, "Nom requis", "Saisissez un nom pour le profil."); return; }
            TypeName = name; Description = txtDesc.Text.Trim(); IconId = selectedIconId; DialogResult = DialogResult.OK; Close();
        }
    }
}
