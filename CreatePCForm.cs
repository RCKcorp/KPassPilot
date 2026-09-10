using System;
using System.Drawing;
using System.Windows.Forms;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Security;

namespace KPassPilot
{
    /// <summary>Création guidée d'un équipement et de ses entrées KeePass.</summary>
    public class CreatePCForm : Form
    {
        private readonly KPassPilotConfig config;
        private readonly IPluginHost host;
        private TextBox txtName, txtSerial;
        private ComboBox cbType;
        private TreeView folders;
        private ListView preview;
        private Label status;
        private Button create;
        private bool typeLocked;

        public CreatePCForm(KPassPilotConfig config, IPluginHost host)
        {
            this.config = config;
            this.host = host;
            BuildUI(); LoadFolders(); RefreshPreview();
        }

        private void BuildUI()
        {
            Text = "KPassPilot — Créer un équipement"; Width = 980; Height = 680; MinimumSize = new Size(860, 600);
            StartPosition = FormStartPosition.CenterScreen; BackColor = Theme.Canvas; Font = Theme.Body;
            var page = Theme.PageGrid(); Controls.Add(page);
            page.Controls.Add(Theme.PageHeader("Créer un équipement", "Exemple public fictif : NORTH.LAPTOP.TEST.4827"), 0, 0);

            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.Canvas, Padding = new Padding(6), Margin = new Padding(0) };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350)); body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.Controls.Add(BuildInput(), 0, 0); body.Controls.Add(BuildPreview(), 1, 0); page.Controls.Add(body, 0, 1);
            page.Controls.Add(BuildFooter(), 0, 2);
        }

        private Control BuildInput()
        {
            var col = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Canvas };
            col.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); col.RowStyles.Add(new RowStyle(SizeType.Absolute, 250)); col.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var idCard = Theme.MakeCard("Identité", false);
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, BackColor = Theme.Surface, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24)); grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24)); grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24)); grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grid.Controls.Add(Caption("Identifiant équipement"), 0, 0);
            txtName = new TextBox { Dock = DockStyle.Fill, Font = Theme.Mono, BorderStyle = BorderStyle.FixedSingle, CharacterCasing = CharacterCasing.Upper };
            txtName.TextChanged += (s, e) => { typeLocked = false; RefreshPreview(); }; grid.Controls.Add(txtName, 0, 1);
            grid.Controls.Add(Caption("Numéro de série"), 0, 2);
            txtSerial = new TextBox { Dock = DockStyle.Fill, Font = Theme.Mono, BorderStyle = BorderStyle.FixedSingle };
            txtSerial.TextChanged += (s, e) => RefreshPreview(); grid.Controls.Add(txtSerial, 0, 3);
            grid.Controls.Add(Caption("Profil"), 0, 4);
            cbType = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Input };
            foreach (var type in config.PCTypes) cbType.Items.Add(type.Name);
            cbType.SelectionChangeCommitted += (s, e) => { typeLocked = true; RefreshPreview(); }; grid.Controls.Add(cbType, 0, 5);
            idCard.Body.Controls.Add(grid); col.Controls.Add(idCard.Root, 0, 0);

            var folderCard = Theme.MakeCard("Dossier KeePass parent", false);
            folders = new TreeView { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, HideSelection = false, Font = Theme.Body, BackColor = Theme.Surface };
            folders.AfterSelect += (s, e) => RefreshPreview(); folderCard.Body.Controls.Add(folders); col.Controls.Add(folderCard.Root, 0, 1);
            return col;
        }

        private Control BuildPreview()
        {
            var card = Theme.MakeCard("Aperçu des entrées", false);
            preview = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, BorderStyle = BorderStyle.None, Font = Theme.Body, BackColor = Theme.Surface, HeaderStyle = ColumnHeaderStyle.Nonclickable };
            preview.Columns.Add("Entrée", 150); preview.Columns.Add("Titre", 170); preview.Columns.Add("Identifiant", 180); preview.Columns.Add("Mot de passe", 210);
            card.Body.Controls.Add(preview); return card.Root;
        }

        private Label Caption(string text) { return new Label { Text = text, Dock = DockStyle.Fill, Font = Theme.BodyBold, ForeColor = Theme.TextPrimary, TextAlign = ContentAlignment.MiddleLeft }; }

        private Control BuildFooter()
        {
            var footer = Theme.PageFooter();
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.SurfaceAlt, Padding = new Padding(14, 10, 14, 10) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            status = new Label { Dock = DockStyle.Fill, Font = Theme.Body, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 8, 0) }; grid.Controls.Add(status, 0, 0);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true, BackColor = Theme.SurfaceAlt };
            var close = Theme.Secondary("Fermer", 100); close.DialogResult = DialogResult.Cancel; actions.Controls.Add(close); CancelButton = close;
            create = Theme.Primary("Créer dans KeePass", 180); create.Enabled = false; create.Click += (s, e) => CreateEntries(); actions.Controls.Add(create);
            grid.Controls.Add(actions, 1, 0); footer.Controls.Add(grid); return footer;
        }

        private void LoadFolders()
        {
            folders.Nodes.Clear(); var db = host == null ? null : host.Database;
            if (db == null || !db.IsOpen) { folders.Nodes.Add("(aucune base ouverte)"); return; }
            var root = new TreeNode(db.RootGroup.Name) { Tag = db.RootGroup }; AddChildren(root, db.RootGroup); folders.Nodes.Add(root); root.Expand(); folders.SelectedNode = root;
        }

        private void AddChildren(TreeNode node, PwGroup group)
        {
            foreach (PwGroup child in group.Groups) { var n = new TreeNode(child.Name) { Tag = child }; AddChildren(n, child); node.Nodes.Add(n); }
        }

        private PwGroup SelectedGroup() { return folders.SelectedNode == null ? null : folders.SelectedNode.Tag as PwGroup; }

        private void RefreshPreview()
        {
            if (preview == null) return; preview.Items.Clear(); create.Enabled = false;
            var parsed = PCGenerator.TryParse(txtName.Text);
            if (parsed == null) { Theme.SetStatus(status, "Format de démo : NORTH.LAPTOP.TEST.4827", 0); return; }

            if (!typeLocked)
            {
                int found = config.PCTypes.FindIndex(t => string.Equals(t.Name, parsed.Category, StringComparison.OrdinalIgnoreCase));
                if (found >= 0) cbType.SelectedIndex = found;
            }
            var type = cbType.SelectedIndex >= 0 && cbType.SelectedIndex < config.PCTypes.Count ? config.PCTypes[cbType.SelectedIndex] : null;
            if (type == null) { Theme.SetStatus(status, "Choisissez un profil pour cet équipement.", 1); return; }

            string serial = txtSerial.Text.Trim();
            foreach (var reference in type.Entries)
            {
                var template = config.Resolve(reference); if (template == null) continue;
                var generated = PCGenerator.Generate(template, parsed, serial, type);
                var item = new ListViewItem(template.Name); item.SubItems.Add(generated.Title); item.SubItems.Add(generated.Username); item.SubItems.Add(generated.Password); preview.Items.Add(item);
            }

            if (!PCGenerator.ValidateSN(serial)) { Theme.SetStatus(status, "Renseignez le numéro de série.", 1); return; }
            Theme.SetStatus(status, "Prêt : " + type.Entries.Count + " entrée(s) pour " + parsed.FullName + ".", 3);
            create.Enabled = host != null && host.Database != null && host.Database.IsOpen;
        }

        private void CreateEntries()
        {
            var db = host == null ? null : host.Database; if (db == null || !db.IsOpen) { Dialogs.Info(this, "Aucune base ouverte", "Ouvrez d'abord une base KeePass."); return; }
            var parsed = PCGenerator.TryParse(txtName.Text); var type = cbType.SelectedIndex >= 0 && cbType.SelectedIndex < config.PCTypes.Count ? config.PCTypes[cbType.SelectedIndex] : null;
            if (parsed == null || type == null) return;
            string serial = txtSerial.Text.Trim(); var parent = SelectedGroup() ?? db.RootGroup;
            if (FindGroup(parent, parsed.FullName) != null) { Dialogs.Info(this, "Équipement déjà présent", "Le dossier « " + parsed.FullName + " » existe déjà."); return; }

            PwIcon icon = type.IconId >= 0 && type.IconId <= 68 ? (PwIcon)type.IconId : PwIcon.Drive;
            var group = new PwGroup(true, true, parsed.FullName, icon); parent.AddGroup(group, true);
            group.Notes = "Créé par KPassPilot (démonstration publique) le " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\r\nN° de série : " + serial;

            int count = 0;
            foreach (var reference in type.Entries)
            {
                var template = config.Resolve(reference); if (template == null) continue;
                var generated = PCGenerator.Generate(template, parsed, serial, type);
                var entry = new PwEntry(true, true);
                entry.Strings.Set(PwDefs.TitleField, new ProtectedString(db.MemoryProtection.ProtectTitle, generated.Title ?? ""));
                entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(db.MemoryProtection.ProtectUserName, generated.Username ?? ""));
                entry.Strings.Set(PwDefs.PasswordField, new ProtectedString(db.MemoryProtection.ProtectPassword, generated.Password ?? ""));
                if (!string.IsNullOrEmpty(generated.Notes)) entry.Strings.Set(PwDefs.NotesField, new ProtectedString(db.MemoryProtection.ProtectNotes, generated.Notes));
                if (generated.IconId >= 0 && generated.IconId <= 68) entry.IconId = (PwIcon)generated.IconId;
                group.AddEntry(entry, true); count++;
            }

            host.MainWindow.UpdateUI(false, null, true, null, true, null, true);
            Dialogs.Info(this, count + " entrée(s) créée(s)", "Créées dans « " + parsed.FullName + " ». Pensez à enregistrer la base KeePass.");
            LoadFolders(); txtName.Text = ""; txtSerial.Text = ""; typeLocked = false; RefreshPreview();
        }

        private static PwGroup FindGroup(PwGroup parent, string name)
        {
            foreach (PwGroup group in parent.Groups) if (string.Equals(group.Name, name, StringComparison.OrdinalIgnoreCase)) return group;
            return null;
        }
    }
}
