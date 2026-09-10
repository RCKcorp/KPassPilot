using System;
using System.Drawing;
using System.Windows.Forms;

namespace KPassPilot
{
    /// <summary>Gestion des fragments globaux utilisés par les modèles publics de démonstration.</summary>
    public class PropertiesForm : Form
    {
        private readonly KPassPilotConfig config;
        private readonly ListBox list;

        public PropertiesForm(KPassPilotConfig config)
        {
            this.config = config;
            Text = "Valeurs globales"; Width = 520; Height = 400; MinimumSize = new Size(480, 360);
            StartPosition = FormStartPosition.CenterParent; BackColor = Theme.Canvas; Font = Theme.Body; ShowInTaskbar = false;

            var page = Theme.PageGrid(); Controls.Add(page);
            page.Controls.Add(Theme.PageHeader("Valeurs globales", "Fragments génériques disponibles lors de la composition d'une entrée."), 0, 0);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1, BackColor = Theme.Canvas, Padding = new Padding(6, 6, 6, 2), Margin = new Padding(0) };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); page.Controls.Add(content, 0, 1);

            var card = Theme.MakeCard("Valeurs définies", true);
            list = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, Font = Theme.Body, BackColor = Theme.Surface };
            card.Body.Controls.Add(list);
            var add = Theme.Secondary("Ajouter", 96); add.Click += (s, e) => Add(); card.Toolbar.Controls.Add(add);
            var edit = Theme.Secondary("Modifier", 96); edit.Click += (s, e) => Edit(); card.Toolbar.Controls.Add(edit);
            var del = Theme.Destructive("Supprimer", 96); del.Click += (s, e) => Remove(); card.Toolbar.Controls.Add(del);
            content.Controls.Add(card.Root, 0, 0);
            page.Controls.Add(BuildFooter(), 0, 2);
            LoadList();
        }

        private Control BuildFooter()
        {
            var footer = Theme.PageFooter();
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.SurfaceAlt, Padding = new Padding(14, 11, 16, 0), Margin = new Padding(0) };
            var close = Theme.Primary("Fermer", 110); close.DialogResult = DialogResult.OK; flow.Controls.Add(close); footer.Controls.Add(flow); AcceptButton = close; return footer;
        }

        private void LoadList()
        {
            int keep = list.SelectedIndex; list.Items.Clear(); foreach (var value in config.GlobalValues) list.Items.Add(value);
            if (list.Items.Count > 0) list.SelectedIndex = Math.Min(Math.Max(keep, 0), list.Items.Count - 1);
        }

        private void Add()
        {
            string value = Dialogs.Prompt(this, "Nouvelle valeur", "Fragment de démonstration (ex : DEMO-, LAB-, -TEST) :", "");
            if (string.IsNullOrEmpty(value)) return; value = value.Trim();
            if (config.GlobalValues.Contains(value)) { Dialogs.Info(this, "Valeur existante", "Cette valeur existe déjà."); return; }
            config.GlobalValues.Add(value); LoadList(); list.SelectedIndex = config.GlobalValues.Count - 1;
        }

        private void Edit()
        {
            int i = list.SelectedIndex; if (i < 0 || i >= config.GlobalValues.Count) return;
            string old = config.GlobalValues[i]; string value = Dialogs.Prompt(this, "Modifier", "Nouvelle valeur :", old);
            if (string.IsNullOrEmpty(value)) return; value = value.Trim(); if (value == old) return;
            if (config.GlobalValues.Contains(value)) { Dialogs.Info(this, "Valeur existante", "Cette valeur existe déjà."); return; }
            config.GlobalValues[i] = value; LoadList();
        }

        private void Remove()
        {
            int i = list.SelectedIndex; if (i < 0 || i >= config.GlobalValues.Count) return;
            string value = config.GlobalValues[i];
            if (!Dialogs.Confirm(this, "Supprimer « " + value + " » ?", "Les blocs qui l'utilisent devront être ajustés.")) return;
            config.GlobalValues.RemoveAt(i); LoadList();
        }
    }
}
