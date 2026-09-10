using System;
using System.Drawing;
using System.Windows.Forms;

namespace KPassPilot
{
    /// <summary>Éditeur visuel d'un bloc de composition.</summary>
    public class BlockEditorForm : Form
    {
        private readonly KPassPilotConfig config;
        private readonly PCType type;
        private readonly PCNameParsed sample;
        private readonly string sampleSn;

        private RadioButton rbText, rbSource, rbProp;
        private Panel pnlText, pnlSource, pnlProp;
        private TextBox txtText;
        private ComboBox cbSource, cbProp;
        private RadioButton rbAll, rbFirst, rbLast, rbSlice;
        private NumericUpDown numFirst, numLast, numFrom, numLen;
        private ComboBox cbCase;
        private Label lblSliceHint, lblPvValue;
        private Control[] sliceControls;

        public Block Result { get; private set; }

        public BlockEditorForm(Block existing, KPassPilotConfig config, PCType type)
        {
            this.config = config;
            this.type = type;
            sample = PCGenerator.TryParse("NORTH.LAPTOP.TEST.4827");
            sampleSn = "DEMO-SN-845921";
            Result = existing == null ? new Block() : existing.Clone();
            BuildUI();
            LoadFrom(Result);
            RefreshPreview();
        }

        private void BuildUI()
        {
            Text = "Morceau"; Width = 620; Height = 740; MinimumSize = new Size(560, 720);
            StartPosition = FormStartPosition.CenterParent; BackColor = Theme.Canvas; Font = Theme.Body; ShowInTaskbar = false;
            var page = Theme.PageGrid(); Controls.Add(page);
            page.Controls.Add(Theme.PageHeader("Morceau", "Un champ se compose de morceaux mis bout à bout. Décrivez celui-ci."), 0, 0);

            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Theme.Canvas, Padding = new Padding(6, 6, 6, 2), Margin = new Padding(0) };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 140f)); body.RowStyles.Add(new RowStyle(SizeType.Absolute, 96f));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); body.RowStyles.Add(new RowStyle(SizeType.Absolute, 88f));
            page.Controls.Add(body, 0, 1);
            body.Controls.Add(BuildKindCard(), 0, 0); body.Controls.Add(BuildDataCard(), 0, 1); body.Controls.Add(BuildSliceCard(), 0, 2); body.Controls.Add(BuildPreview(), 0, 3);
            page.Controls.Add(BuildFooter(), 0, 2);
        }

        private Control BuildKindCard()
        {
            var card = Theme.MakeCard("1 · Quel genre de morceau ?", false);
            var stack = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Theme.Surface, Padding = new Padding(11, 8, 11, 8), Margin = new Padding(0) };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 3; i++) stack.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
            rbText = Choice("Du texte fixe", "Toujours identique, par exemple « DEMO- » ou « -ops ».");
            rbSource = Choice("Une donnée de l'équipement", "Recalculée pour chaque équipement créé.");
            rbProp = Choice("Une valeur globale", "Fragment configurable partagé par les profils de démonstration.");
            stack.Controls.Add(rbText, 0, 0); stack.Controls.Add(rbSource, 0, 1); stack.Controls.Add(rbProp, 0, 2);
            rbText.CheckedChanged += KindChanged; rbSource.CheckedChanged += KindChanged; rbProp.CheckedChanged += KindChanged;
            card.Body.Controls.Add(stack); return card.Root;
        }

        private RadioButton Choice(string title, string hint)
        {
            var rb = new RadioButton { Dock = DockStyle.Fill, Font = Theme.BodyBold, ForeColor = Theme.TextPrimary, Cursor = Cursors.Hand, Margin = new Padding(0), Padding = new Padding(0), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, Text = title };
            new ToolTip().SetToolTip(rb, hint);
            return rb;
        }

        private Control BuildDataCard()
        {
            var card = Theme.MakeCard("2 · Quelle donnée ?", false);
            var host = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface, Padding = new Padding(11, 12, 11, 10) };

            pnlText = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };
            txtText = new TextBox { Dock = DockStyle.Top, Font = Theme.Mono, BorderStyle = BorderStyle.FixedSingle };
            txtText.TextChanged += (s, e) => RefreshPreview(); pnlText.Controls.Add(txtText); host.Controls.Add(pnlText);

            pnlSource = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };
            cbSource = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Input };
            foreach (var id in SourceIds.All) cbSource.Items.Add(SourceIds.Label(id));
            cbSource.SelectedIndexChanged += (s, e) => RefreshPreview(); pnlSource.Controls.Add(cbSource); host.Controls.Add(pnlSource);

            pnlProp = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };
            cbProp = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Input };
            foreach (var v in config.GlobalValues) cbProp.Items.Add(v);
            cbProp.SelectedIndexChanged += (s, e) => RefreshPreview(); pnlProp.Controls.Add(cbProp);
            if (config.GlobalValues.Count == 0)
                pnlProp.Controls.Add(new Label { Dock = DockStyle.Bottom, Height = 18, Font = Theme.Small, ForeColor = Theme.WarnText, Text = "Aucune valeur globale. Ajoutez-en une depuis la configuration." });
            host.Controls.Add(pnlProp);

            card.Body.Controls.Add(host); return card.Root;
        }

        private Control BuildSliceCard()
        {
            var card = Theme.MakeCard("3 · Quelle partie garder ?", false);
            var stack = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, BackColor = Theme.Surface, Padding = new Padding(11, 8, 11, 8), Margin = new Padding(0) };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 4; i++) stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f)); stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f)); stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            rbAll = Radio("Tout"); rbFirst = Radio("Les"); rbLast = Radio("Les"); rbSlice = Radio("À partir du caractère");
            numFirst = Num(1, 3); numLast = Num(1, 3); numFrom = Num(1, 1); numLen = Num(1, 2);
            stack.Controls.Add(Row(rbAll), 0, 0); stack.Controls.Add(Row(rbFirst, numFirst, Word("premiers caractères")), 0, 1);
            stack.Controls.Add(Row(rbLast, numLast, Word("derniers caractères")), 0, 2);
            stack.Controls.Add(Row(rbSlice, numFrom, Word("sur"), numLen, Word("caractères")), 0, 3);

            lblSliceHint = new Label { Dock = DockStyle.Fill, Font = Theme.Small, ForeColor = Theme.TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            stack.Controls.Add(lblSliceHint, 0, 4);

            var caseRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = Theme.Surface, WrapContents = false, Margin = new Padding(0, 6, 0, 0) };
            caseRow.Controls.Add(new Label { Text = "Casse :", Font = Theme.BodyBold, ForeColor = Theme.TextPrimary, Width = 54, Height = 26, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 2, 4, 0) });
            cbCase = new ComboBox { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Body, Margin = new Padding(0, 2, 0, 0) };
            cbCase.Items.AddRange(new object[] { "Inchangée", "MAJUSCULES", "minuscules" }); cbCase.SelectedIndex = 0; cbCase.SelectedIndexChanged += (s, e) => RefreshPreview();
            caseRow.Controls.Add(cbCase); stack.Controls.Add(caseRow, 0, 5);

            rbAll.CheckedChanged += (s, e) => { if (rbAll.Checked) { rbFirst.Checked = rbLast.Checked = rbSlice.Checked = false; RefreshPreview(); } };
            rbFirst.CheckedChanged += (s, e) => { if (rbFirst.Checked) { rbAll.Checked = rbLast.Checked = rbSlice.Checked = false; RefreshPreview(); } };
            rbLast.CheckedChanged += (s, e) => { if (rbLast.Checked) { rbAll.Checked = rbFirst.Checked = rbSlice.Checked = false; RefreshPreview(); } };
            rbSlice.CheckedChanged += (s, e) => { if (rbSlice.Checked) { rbAll.Checked = rbFirst.Checked = rbLast.Checked = false; RefreshPreview(); } };
            sliceControls = new Control[] { rbAll, rbFirst, rbLast, rbSlice, numFirst, numLast, numFrom, numLen };
            card.Body.Controls.Add(stack); return card.Root;
        }

        private RadioButton Radio(string text) { return new RadioButton { Text = text, AutoSize = true, Font = Theme.Body, ForeColor = Theme.TextPrimary, Cursor = Cursors.Hand, Margin = new Padding(0, 4, 6, 0) }; }
        private NumericUpDown Num(int min, int val) { var n = new NumericUpDown { Width = 54, Minimum = min, Maximum = 64, Value = val, Font = Theme.Body, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 2, 6, 0) }; n.ValueChanged += (s, e) => RefreshPreview(); return n; }
        private Label Word(string text) { return new Label { Text = text, AutoSize = true, Font = Theme.Body, ForeColor = Theme.TextPrimary, Margin = new Padding(0, 7, 8, 0) }; }
        private Control Row(params Control[] items) { var f = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = Theme.Surface, WrapContents = false, Margin = new Padding(0) }; foreach (var c in items) f.Controls.Add(c); return f; }

        private Control BuildPreview()
        {
            var box = new Panel { Dock = DockStyle.Fill, BackColor = Theme.AccentSoft, Padding = new Padding(13, 9, 13, 9), Margin = new Padding(5, 2, 5, 4) };
            box.Paint += (s, e) => { using (var p = new Pen(Theme.AccentBorder)) e.Graphics.DrawRectangle(p, 0, 0, box.Width - 1, box.Height - 1); };
            lblPvValue = new Label { Dock = DockStyle.Fill, Font = new Font("Consolas", 13f, FontStyle.Bold), ForeColor = Theme.AccentText, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            box.Controls.Add(lblPvValue);
            box.Controls.Add(new Label { Dock = DockStyle.Top, Height = 17, Font = Theme.Small, ForeColor = Theme.TextMuted, Text = "Aperçu public : NORTH.LAPTOP.TEST.4827 · DEMO-SN-845921" });
            return box;
        }

        private Control BuildFooter()
        {
            var footer = Theme.PageFooter();
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.SurfaceAlt, Padding = new Padding(14, 11, 16, 0), Margin = new Padding(0) };
            var ok = Theme.Primary("Valider", 116); ok.DialogResult = DialogResult.OK; ok.Click += (s, e) => Result = BuildBlock(); flow.Controls.Add(ok);
            var cancel = Theme.Secondary("Annuler", 100); cancel.Height = 34; cancel.DialogResult = DialogResult.Cancel; flow.Controls.Add(cancel);
            footer.Controls.Add(flow); AcceptButton = ok; CancelButton = cancel; return footer;
        }

        private void KindChanged(object sender, EventArgs e)
        {
            pnlText.Visible = rbText.Checked; pnlSource.Visible = rbSource.Checked; pnlProp.Visible = rbProp.Checked;
            bool sliceable = rbSource.Checked; foreach (var c in sliceControls) c.Enabled = sliceable; if (!sliceable) rbAll.Checked = true;
            lblSliceHint.Text = sliceable ? "Les bornes hors limites sont ramenées à la taille réelle de la donnée." : "La découpe s'applique uniquement aux données d'équipement.";
            RefreshPreview();
        }

        private void LoadFrom(Block b)
        {
            txtText.Text = b.Value ?? string.Empty;
            int si = Array.IndexOf(SourceIds.All, b.Src); cbSource.SelectedIndex = si < 0 ? 1 : si;
            int pi = config.GlobalValues.FindIndex(x => x == b.Key); cbProp.SelectedIndex = config.GlobalValues.Count == 0 ? -1 : (pi < 0 ? 0 : pi);
            int n = Math.Max(1, Math.Min(64, b.N)); numFirst.Value = n; numLast.Value = n; numFrom.Value = Math.Max(1, Math.Min(64, b.From)); numLen.Value = Math.Max(1, Math.Min(64, b.Len));
            if (b.Mode == SliceMode.First) rbFirst.Checked = true; else if (b.Mode == SliceMode.Last) rbLast.Checked = true; else if (b.Mode == SliceMode.Slice) rbSlice.Checked = true; else rbAll.Checked = true;
            cbCase.SelectedIndex = b.Case == CaseMode.Upper ? 1 : (b.Case == CaseMode.Lower ? 2 : 0);
            if (b.Kind == BlockKind.Source) rbSource.Checked = true; else if (b.Kind == BlockKind.Prop) rbProp.Checked = true; else rbText.Checked = true;
            KindChanged(null, null);
        }

        private Block BuildBlock()
        {
            var b = new Block();
            if (rbSource.Checked) b.Kind = BlockKind.Source; else if (rbProp.Checked) b.Kind = BlockKind.Prop; else b.Kind = BlockKind.Text;
            b.Value = txtText.Text;
            b.Src = cbSource.SelectedIndex >= 0 ? SourceIds.All[cbSource.SelectedIndex] : SourceIds.Entity;
            b.Key = cbProp.SelectedIndex >= 0 && cbProp.SelectedIndex < config.GlobalValues.Count ? config.GlobalValues[cbProp.SelectedIndex] : string.Empty;
            if (rbFirst.Checked) { b.Mode = SliceMode.First; b.N = (int)numFirst.Value; }
            else if (rbLast.Checked) { b.Mode = SliceMode.Last; b.N = (int)numLast.Value; }
            else if (rbSlice.Checked) { b.Mode = SliceMode.Slice; b.N = (int)numFirst.Value; }
            else { b.Mode = SliceMode.All; b.N = (int)numFirst.Value; }
            b.From = (int)numFrom.Value; b.Len = (int)numLen.Value;
            b.Case = cbCase.SelectedIndex == 1 ? CaseMode.Upper : cbCase.SelectedIndex == 2 ? CaseMode.Lower : CaseMode.None;
            return b;
        }

        private void RefreshPreview()
        {
            if (lblPvValue == null) return;
            var b = BuildBlock(); string value = PCGenerator.BlockValue(b, sample, sampleSn, type);
            if (string.IsNullOrEmpty(value)) { lblPvValue.Text = "(vide)"; lblPvValue.ForeColor = Theme.TextMuted; }
            else { lblPvValue.Text = value; lblPvValue.ForeColor = Theme.AccentText; }
        }
    }
}
