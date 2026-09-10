using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KeePass.Plugins;

namespace KPassPilot
{
    /// <summary>Éditeur visuel public des profils, modèles et blocs.</summary>
    public class ConfigEditorForm : Form
    {
        private readonly KPassPilotConfig config;
        private readonly ImageList keepassIcons;
        private readonly Action onSave;

        private ListBox lbTypes, lbEntries, lbModels;
        private TextBox txtEntryName, txtSample;
        private Button btnIcon;
        private Label lblContext, lblPreview;
        private readonly Dictionary<string, FlowLayoutPanel> strips = new Dictionary<string, FlowLayoutPanel>();
        private bool loading;

        private static readonly string[] FieldKeys = { "title", "user", "pass", "notes" };
        private static readonly string[] FieldNames = { "Titre", "Identifiant", "Mot de passe", "Notes" };

        public ConfigEditorForm(KPassPilotConfig config, IPluginHost host, Action onSave = null)
        {
            this.config = config;
            this.onSave = onSave;
            keepassIcons = ResolveIcons(host);
            BuildUI();
            LoadTypes();
            LoadModels();
        }

        private static ImageList ResolveIcons(IPluginHost host)
        {
            try { return host == null || host.MainWindow == null ? null : host.MainWindow.ClientIcons; }
            catch { return null; }
        }

        private void Save() { if (onSave != null) onSave(); }

        private void BuildUI()
        {
            Text = "KPassPilot — Configuration";
            Width = 1260; Height = 800; MinimumSize = new Size(1020, 650);
            StartPosition = FormStartPosition.CenterScreen; BackColor = Theme.Canvas; Font = Theme.Body;

            var page = Theme.PageGrid(); Controls.Add(page);
            page.Controls.Add(Theme.PageHeader("Configuration", "Dépôt public : profils et exemples fictifs, moteur entièrement configurable."), 0, 0);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Theme.Canvas, Padding = new Padding(6), Margin = new Padding(0) };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 285));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.Controls.Add(BuildProfilesColumn(), 0, 0);
            content.Controls.Add(BuildEntriesColumn(), 1, 0);
            content.Controls.Add(BuildEditorColumn(), 2, 0);
            page.Controls.Add(content, 0, 1);
            page.Controls.Add(BuildFooter(), 0, 2);
        }

        private Control BuildProfilesColumn()
        {
            var col = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Canvas, Margin = new Padding(0) };
            col.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            col.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            col.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            var profiles = Theme.MakeCard("Profils", true);
            lbTypes = NewList();
            lbTypes.SelectedIndexChanged += (s, e) => { if (!loading) { loading = true; lbModels.ClearSelected(); loading = false; LoadEntries(); } };
            profiles.Body.Controls.Add(lbTypes);
            profiles.Toolbar.Controls.Add(ActionButton("Ajouter", AddType));
            profiles.Toolbar.Controls.Add(ActionButton("Modifier", EditType));
            var deleteType = Theme.Destructive("Supprimer", 82); deleteType.Click += (s, e) => RemoveType(); profiles.Toolbar.Controls.Add(deleteType);
            col.Controls.Add(profiles.Root, 0, 0);

            var models = Theme.MakeCard("Modèles partagés", true);
            lbModels = NewList();
            lbModels.SelectedIndexChanged += (s, e) =>
            {
                if (loading || lbModels.SelectedIndex < 0) return;
                loading = true; lbEntries.ClearSelected(); loading = false; LoadCurrentEntry();
            };
            models.Body.Controls.Add(lbModels);
            models.Toolbar.Controls.Add(ActionButton("Nouveau", AddModel));
            var deleteModel = Theme.Destructive("Supprimer", 82); deleteModel.Click += (s, e) => RemoveModel(); models.Toolbar.Controls.Add(deleteModel);
            col.Controls.Add(models.Root, 0, 1);
            return col;
        }

        private Control BuildEntriesColumn()
        {
            var col = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Canvas, Margin = new Padding(0) };
            col.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            col.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
            col.RowStyles.Add(new RowStyle(SizeType.Percent, 35));

            var entries = Theme.MakeCard("Entrées du profil", true);
            lbEntries = NewList();
            lbEntries.SelectedIndexChanged += (s, e) =>
            {
                if (loading || lbEntries.SelectedIndex < 0) return;
                loading = true; lbModels.ClearSelected(); loading = false; LoadCurrentEntry();
            };
            entries.Body.Controls.Add(lbEntries);
            entries.Toolbar.Controls.Add(ActionButton("Ajouter", AddEntry));
            entries.Toolbar.Controls.Add(ActionButton("Modèle", UseModel));
            var remove = Theme.Destructive("Retirer", 70); remove.Click += (s, e) => RemoveEntry(); entries.Toolbar.Controls.Add(remove);
            col.Controls.Add(entries.Root, 0, 0);

            var globals = Theme.MakeCard("Valeurs globales", false);
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface, Padding = new Padding(12) };
            var info = new Label { Dock = DockStyle.Top, Height = 95, Font = Theme.Small, ForeColor = Theme.TextMuted, Text = "Fragments optionnels réutilisables dans les modèles.\r\n\r\nExemples publics : DEMO-, LAB-, -TEST." };
            p.Controls.Add(info);
            var manage = Theme.Secondary("Gérer les valeurs", 150); manage.Top = 105; manage.Left = 12; manage.Click += (s, e) => ManageValues(); p.Controls.Add(manage);
            globals.Body.Controls.Add(p);
            col.Controls.Add(globals.Root, 0, 1);
            return col;
        }

        private Control BuildEditorColumn()
        {
            var card = Theme.MakeCard("Composition de l'entrée", false);
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Theme.Surface, Padding = new Padding(12), Margin = new Padding(0) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));

            var head = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, BackColor = Theme.Surface };
            head.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45)); head.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); head.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48)); head.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
            head.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); head.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            head.Controls.Add(new Label { Text = "Nom", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = Theme.BodyBold }, 0, 0);
            txtEntryName = new TextBox { Dock = DockStyle.Fill, Font = Theme.Input, BorderStyle = BorderStyle.FixedSingle };
            txtEntryName.TextChanged += (s, e) => RenameEntry(); head.Controls.Add(txtEntryName, 1, 0);
            head.Controls.Add(new Label { Text = "Icône", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = Theme.Small }, 2, 0);
            btnIcon = Theme.Secondary("—", 58); btnIcon.Click += (s, e) => PickIcon(); head.Controls.Add(btnIcon, 3, 0);
            lblContext = new Label { Dock = DockStyle.Fill, Font = Theme.Small, ForeColor = Theme.TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            head.Controls.Add(lblContext, 0, 1); head.SetColumnSpan(lblContext, 4);
            grid.Controls.Add(head, 0, 0);

            var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, AutoScroll = true, BackColor = Theme.Surface };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < FieldKeys.Length; i++)
            {
                fields.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
                fields.Controls.Add(BuildField(FieldKeys[i], FieldNames[i]), 0, i);
            }
            grid.Controls.Add(fields, 0, 1);

            var samplePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.SurfaceAlt, Padding = new Padding(8) };
            samplePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); samplePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            samplePanel.Controls.Add(new Label { Text = "Exemple public", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = Theme.BodyBold }, 0, 0);
            txtSample = new TextBox { Text = "NORTH.LAPTOP.TEST.4827", Dock = DockStyle.Fill, Font = Theme.Mono, CharacterCasing = CharacterCasing.Upper, BorderStyle = BorderStyle.FixedSingle };
            txtSample.TextChanged += (s, e) => RefreshPreview(); samplePanel.Controls.Add(txtSample, 1, 0);
            grid.Controls.Add(samplePanel, 0, 2);

            lblPreview = new Label { Dock = DockStyle.Fill, Font = Theme.MonoSmall, ForeColor = Theme.TextPrimary, BackColor = Theme.SurfaceAlt, Padding = new Padding(10), AutoEllipsis = true };
            grid.Controls.Add(lblPreview, 0, 3);
            card.Body.Controls.Add(grid); return card.Root;
        }

        private Control BuildField(string key, string name)
        {
            var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Surface, Margin = new Padding(0, 0, 0, 5) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); row.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            row.Controls.Add(new Label { Text = name, Dock = DockStyle.Fill, Font = Theme.BodyBold, ForeColor = Theme.TextPrimary, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            var strip = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true, BackColor = Theme.SurfaceAlt, Padding = new Padding(6), Tag = key };
            strips[key] = strip; row.Controls.Add(strip, 0, 1); return row;
        }

        private Control BuildFooter()
        {
            var footer = Theme.PageFooter();
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.SurfaceAlt, Padding = new Padding(14, 11, 16, 0) };
            var close = Theme.Primary("Fermer", 110); close.DialogResult = DialogResult.OK; flow.Controls.Add(close); footer.Controls.Add(flow); AcceptButton = close; CancelButton = close; return footer;
        }

        private ListBox NewList() { return new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, Font = Theme.Body, BackColor = Theme.Surface, IntegralHeight = false }; }
        private Button ActionButton(string text, Action action) { var b = Theme.Secondary(text, 74); b.Click += (s, e) => action(); return b; }

        private PCType CurrentType()
        {
            int i = lbTypes.SelectedIndex; return i >= 0 && i < config.PCTypes.Count ? config.PCTypes[i] : null;
        }

        private EntryTemplate CurrentEntry()
        {
            if (lbModels.SelectedIndex >= 0 && lbModels.SelectedIndex < config.Models.Count) return config.Models[lbModels.SelectedIndex];
            var type = CurrentType(); int i = lbEntries.SelectedIndex;
            return type != null && i >= 0 && i < type.Entries.Count ? config.Resolve(type.Entries[i]) : null;
        }

        private EntryRef CurrentRef()
        {
            if (lbModels.SelectedIndex >= 0) return null;
            var type = CurrentType(); int i = lbEntries.SelectedIndex;
            return type != null && i >= 0 && i < type.Entries.Count ? type.Entries[i] : null;
        }

        private void LoadTypes()
        {
            int keep = lbTypes.SelectedIndex; loading = true; lbTypes.Items.Clear(); foreach (var type in config.PCTypes) lbTypes.Items.Add(type.Name); loading = false;
            if (lbTypes.Items.Count > 0) lbTypes.SelectedIndex = Math.Min(Math.Max(keep, 0), lbTypes.Items.Count - 1); else LoadEntries();
        }

        private void LoadModels()
        {
            int keep = lbModels.SelectedIndex; loading = true; lbModels.Items.Clear(); foreach (var model in config.Models) lbModels.Items.Add(model.Name); loading = false;
            if (keep >= 0 && keep < lbModels.Items.Count) lbModels.SelectedIndex = keep;
        }

        private void LoadEntries()
        {
            var type = CurrentType(); loading = true; lbEntries.Items.Clear();
            if (type != null) foreach (var entry in type.Entries) { var resolved = config.Resolve(entry); lbEntries.Items.Add(resolved == null ? "(modèle introuvable)" : resolved.Name + (entry.IsShared ? "  [partagé]" : "")); }
            loading = false;
            if (lbEntries.Items.Count > 0) lbEntries.SelectedIndex = 0; else LoadCurrentEntry();
        }

        private void LoadCurrentEntry()
        {
            var entry = CurrentEntry(); loading = true; txtEntryName.Text = entry == null ? "" : (entry.Name ?? ""); loading = false;
            txtEntryName.Enabled = btnIcon.Enabled = entry != null;
            if (entry == null)
            {
                btnIcon.Text = "—"; btnIcon.Image = null; lblContext.Text = "Sélectionnez une entrée ou un modèle.";
                foreach (var strip in strips.Values) strip.Controls.Clear(); RefreshPreview(); return;
            }
            SetIcon(entry.IconId);
            var reference = CurrentRef();
            lblContext.Text = lbModels.SelectedIndex >= 0 ? "Modèle partagé." : reference != null && reference.IsShared ? "Référence vers un modèle partagé." : "Entrée propre à ce profil.";
            RefreshStrips(); RefreshPreview();
        }

        private void SetIcon(int id)
        {
            if (keepassIcons != null && id >= 0 && id < keepassIcons.Images.Count) { btnIcon.Image = keepassIcons.Images[id]; btnIcon.Text = ""; }
            else { btnIcon.Image = null; btnIcon.Text = "n° " + id; }
        }

        private void PickIcon()
        {
            var entry = CurrentEntry(); if (entry == null) return;
            using (var dialog = new IconPickerForm(entry.IconId, keepassIcons))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                entry.IconId = dialog.SelectedIcon; SetIcon(entry.IconId); Save();
            }
        }

        private void RefreshStrips()
        {
            var entry = CurrentEntry(); if (entry == null) return;
            foreach (var key in FieldKeys) BuildStrip(key, entry);
        }

        private void BuildStrip(string field, EntryTemplate entry)
        {
            var strip = strips[field]; strip.Controls.Clear(); var blocks = entry.Field(field);
            for (int i = 0; i < blocks.Count; i++) strip.Controls.Add(MakeChip(blocks[i], field, i));
            var add = Theme.Secondary("+ Ajouter", 88); add.Height = 32; add.Tag = field; add.Click += (s, e) => AddBlock((string)((Button)s).Tag); strip.Controls.Add(add);
        }

        private Control MakeChip(Block block, string field, int index)
        {
            string value = PCGenerator.BlockValue(block, PCGenerator.TryParse(txtSample == null ? "NORTH.LAPTOP.TEST.4827" : txtSample.Text), "DEMO-SN-845921", CurrentType());
            string caption = block.Kind == BlockKind.Text ? "Texte: " + (block.Value ?? "") : PCGenerator.BlockLabel(block, config) + " → " + value;
            var b = Theme.Quiet(caption, Math.Max(110, Math.Min(230, TextRenderer.MeasureText(caption, Theme.Small).Width + 20))); b.Height = 32; b.Tag = field + "|" + index;
            b.Click += (s, e) => EditTagged((Button)s);
            var menu = new ContextMenuStrip(); menu.Items.Add("Modifier", null, (s, e) => EditBlock(field, index)); menu.Items.Add("Supprimer", null, (s, e) => RemoveBlock(field, index)); b.ContextMenuStrip = menu;
            return b;
        }

        private void EditTagged(Button button)
        {
            string[] parts = ((string)button.Tag).Split('|'); EditBlock(parts[0], int.Parse(parts[1]));
        }

        private void AddBlock(string field)
        {
            var entry = CurrentEntry(); if (entry == null) return;
            using (var dialog = new BlockEditorForm(null, config, CurrentType()))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                entry.Field(field).Add(dialog.Result); BuildStrip(field, entry); RefreshPreview(); Save();
            }
        }

        private void EditBlock(string field, int index)
        {
            var entry = CurrentEntry(); if (entry == null) return; var list = entry.Field(field); if (index < 0 || index >= list.Count) return;
            using (var dialog = new BlockEditorForm(list[index], config, CurrentType()))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                list[index] = dialog.Result; BuildStrip(field, entry); RefreshPreview(); Save();
            }
        }

        private void RemoveBlock(string field, int index)
        {
            var entry = CurrentEntry(); if (entry == null) return; var list = entry.Field(field); if (index < 0 || index >= list.Count) return;
            list.RemoveAt(index); BuildStrip(field, entry); RefreshPreview(); Save();
        }

        private void RefreshPreview()
        {
            if (lblPreview == null) return;
            var entry = CurrentEntry(); var parsed = PCGenerator.TryParse(txtSample == null ? "NORTH.LAPTOP.TEST.4827" : txtSample.Text);
            if (entry == null) { lblPreview.Text = "Aucun aperçu."; return; }
            if (parsed == null) { lblPreview.Text = "Format de démo invalide. Exemple : NORTH.LAPTOP.TEST.4827"; return; }
            var generated = PCGenerator.Generate(entry, parsed, "DEMO-SN-845921", CurrentType());
            lblPreview.Text = "Titre : " + generated.Title + "\r\nIdentifiant : " + generated.Username + "\r\nMot de passe : " + generated.Password + "\r\nNotes : " + generated.Notes;
        }

        private void AddType()
        {
            using (var dialog = new TypeEditorForm("", "", 0, keepassIcons))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                config.PCTypes.Add(new PCType { Name = dialog.TypeName.ToUpperInvariant(), Description = dialog.Description, IconId = dialog.IconId });
                LoadTypes(); lbTypes.SelectedIndex = config.PCTypes.Count - 1; Save();
            }
        }

        private void EditType()
        {
            var type = CurrentType(); if (type == null) return;
            using (var dialog = new TypeEditorForm(type.Name, type.Description ?? "", type.IconId, keepassIcons))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                type.Name = dialog.TypeName.ToUpperInvariant(); type.Description = dialog.Description; type.IconId = dialog.IconId; LoadTypes(); Save();
            }
        }

        private void RemoveType()
        {
            var type = CurrentType(); if (type == null) return;
            if (!Dialogs.Confirm(this, "Supprimer le profil " + type.Name + " ?", "Ses entrées locales seront supprimées.")) return;
            config.PCTypes.Remove(type); LoadTypes(); Save();
        }

        private void AddEntry()
        {
            var type = CurrentType(); if (type == null) return;
            string name = Dialogs.Prompt(this, "Nouvelle entrée", "Nom de l'entrée (ex : Compte de maintenance) :", "");
            if (string.IsNullOrEmpty(name)) return;
            type.Entries.Add(EntryRef.Local(new EntryTemplate { Name = name.Trim(), IconId = 0 })); LoadEntries(); lbEntries.SelectedIndex = type.Entries.Count - 1; Save();
        }

        private void RemoveEntry()
        {
            var type = CurrentType(); int i = lbEntries.SelectedIndex; if (type == null || i < 0 || i >= type.Entries.Count) return;
            type.Entries.RemoveAt(i); LoadEntries(); Save();
        }

        private void UseModel()
        {
            var type = CurrentType(); if (type == null) return;
            if (config.Models.Count == 0) { Dialogs.Info(this, "Aucun modèle", "Créez d'abord un modèle partagé."); return; }
            var names = new List<string>(); foreach (var model in config.Models) names.Add(model.Name);
            int i = Dialogs.Pick(this, "Ajouter un modèle", "Modèle à ajouter à " + type.Name + " :", names); if (i < 0) return;
            type.Entries.Add(EntryRef.ToModel(config.Models[i].Id)); LoadEntries(); Save();
        }

        private void AddModel()
        {
            string name = Dialogs.Prompt(this, "Nouveau modèle", "Nom du modèle partagé :", ""); if (string.IsNullOrEmpty(name)) return;
            config.Models.Add(new EntryTemplate { Id = "m" + Guid.NewGuid().ToString("N").Substring(0, 8), Name = name.Trim(), IconId = 0 });
            LoadModels(); lbModels.SelectedIndex = config.Models.Count - 1; Save();
        }

        private void RemoveModel()
        {
            int i = lbModels.SelectedIndex; if (i < 0 || i >= config.Models.Count) return;
            var model = config.Models[i]; var users = config.TypesUsing(model.Id);
            if (users.Count > 0) { Dialogs.Info(this, "Modèle utilisé", "Retirez ce modèle des profils avant de le supprimer."); return; }
            if (!Dialogs.Confirm(this, "Supprimer « " + model.Name + " » ?", "")) return;
            config.Models.RemoveAt(i); LoadModels(); LoadCurrentEntry(); Save();
        }

        private void RenameEntry()
        {
            if (loading) return; var entry = CurrentEntry(); if (entry == null || entry.Name == txtEntryName.Text) return;
            entry.Name = txtEntryName.Text; LoadEntries(); LoadModels(); Save();
        }

        private void ManageValues()
        {
            using (var form = new PropertiesForm(config)) form.ShowDialog(this);
            if (CurrentEntry() != null) { RefreshStrips(); RefreshPreview(); } Save();
        }
    }
}
