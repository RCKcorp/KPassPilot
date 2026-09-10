using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace KPassPilot
{
    /// <summary>Informations et import/export de la configuration publique.</summary>
    public class AboutForm : Form
    {
        private readonly KPassPilotConfig config;
        public AboutForm(KPassPilotConfig config)
        {
            this.config = config;
            Text = "À propos de KPassPilot"; Width = 620; Height = 440; StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false; BackColor = Theme.Canvas; Font = Theme.Body;

            var page = Theme.PageGrid(); Controls.Add(page);
            page.Controls.Add(Theme.PageHeader("KPassPilot", "Plugin KeePass — version publique de démonstration"), 0, 0);

            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Theme.Canvas, Padding = new Padding(20) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.Controls.Add(new Label { AutoSize = true, MaximumSize = new Size(550, 0), Font = Theme.Body, ForeColor = Theme.TextPrimary, Text = "Tous les profils, conventions et règles présents dans le dépôt public sont fictifs. La configuration réelle doit rester hors du dépôt Git." }, 0, 0);
            panel.Controls.Add(new Label { AutoSize = true, Font = Theme.Small, ForeColor = Theme.TextMuted, Margin = new Padding(0, 14, 0, 8), Text = "Configuration locale : " + GetConfigPath() }, 0, 1);
            var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, BackColor = Theme.Canvas };
            var export = Theme.Secondary("Exporter...", 110); export.Click += (s, e) => Export(); actions.Controls.Add(export);
            var import = Theme.Secondary("Importer...", 110); import.Click += (s, e) => Import(); actions.Controls.Add(import);
            panel.Controls.Add(actions, 0, 2); page.Controls.Add(panel, 0, 1);

            var footer = Theme.PageFooter(); var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.SurfaceAlt, Padding = new Padding(14, 11, 16, 0) };
            var close = Theme.Primary("Fermer", 110); close.DialogResult = DialogResult.OK; flow.Controls.Add(close); footer.Controls.Add(flow); page.Controls.Add(footer, 0, 2); AcceptButton = close;
        }

        private string GetConfigPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KPassPilot", "KPassPilot.config.xml");
        }

        private void Export()
        {
            using (var dialog = new SaveFileDialog { Filter = "Fichiers XML (*.xml)|*.xml", FileName = "KPassPilot.demo.config.xml" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try { var serializer = new XmlSerializer(typeof(KPassPilotConfig)); using (var writer = new StreamWriter(dialog.FileName)) serializer.Serialize(writer, config); Dialogs.Info(this, "Export terminé", dialog.FileName); }
                catch (Exception ex) { Dialogs.Info(this, "Erreur", ex.Message); }
            }
        }

        private void Import()
        {
            using (var dialog = new OpenFileDialog { Filter = "Fichiers XML (*.xml)|*.xml" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (!Dialogs.Confirm(this, "Importer cette configuration ?", "La configuration courante sera remplacée.")) return;
                try
                {
                    var serializer = new XmlSerializer(typeof(KPassPilotConfig)); KPassPilotConfig imported;
                    using (var reader = new StreamReader(dialog.FileName)) imported = (KPassPilotConfig)serializer.Deserialize(reader);
                    config.GlobalValues.Clear(); foreach (var v in imported.GlobalValues) config.GlobalValues.Add(v);
                    config.Models.Clear(); foreach (var m in imported.Models) config.Models.Add(m);
                    config.PCTypes.Clear(); foreach (var t in imported.PCTypes) config.PCTypes.Add(t);
                    DialogResult = DialogResult.OK; Close();
                }
                catch (Exception ex) { Dialogs.Info(this, "Erreur d'import", ex.Message); }
            }
        }
    }
}
