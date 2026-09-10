using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using KeePass.Plugins;

namespace KPassPilot
{
    public sealed class KPassPilotExt : Plugin
    {
        private IPluginHost host;
        private ToolStripMenuItem menu;
        private KPassPilotConfig config;
        private const string ConfigFileName = "KPassPilot.config.xml";

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            this.host = host;
            LoadConfiguration();
            return true;
        }

        public override void Terminate() { }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType type)
        {
            if (type != PluginMenuType.Main) return null;
            if (menu == null) CreateMenu();
            return menu;
        }

        private string GetConfigPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KPassPilot", ConfigFileName);
        }

        private void LoadConfiguration()
        {
            string path = GetConfigPath();
            try
            {
                if (!File.Exists(path)) { config = CreateDemoConfig(); SaveConfiguration(); return; }
                var serializer = new XmlSerializer(typeof(KPassPilotConfig));
                using (var stream = File.OpenRead(path)) config = (KPassPilotConfig)serializer.Deserialize(stream);
                if (config == null) config = CreateDemoConfig();
            }
            catch
            {
                // Une configuration invalide n'est jamais publiée dans le dépôt : on repart sur la démo locale.
                config = CreateDemoConfig();
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                string path = GetConfigPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var serializer = new XmlSerializer(typeof(KPassPilotConfig));
                using (var stream = File.Create(path)) serializer.Serialize(stream, config);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible d'enregistrer la configuration :\n" + ex.Message, "KPassPilot", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Configuration uniquement destinée à la démonstration publique.
        /// Les noms, formats et fragments ci-dessous sont fictifs et ne doivent pas être utilisés comme politique de sécurité.
        /// </summary>
        private static KPassPilotConfig CreateDemoConfig()
        {
            var cfg = new KPassPilotConfig();
            cfg.GlobalValues.Add("DEMO-");
            cfg.GlobalValues.Add("-EXAMPLE");

            var maintenance = new EntryTemplate { Id = "demo-maintenance", Name = "Compte de maintenance", IconId = 19 };
            maintenance.Title.Add(Block.Text("Maintenance - "));
            maintenance.Title.Add(Block.Source(SourceIds.PcName));
            maintenance.Username.Add(Block.Text("demo-"));
            maintenance.Username.Add(Block.Source(SourceIds.Number, SliceMode.All, 3, CaseMode.Lower));
            maintenance.Password.Add(Block.Text("NOT-A-REAL-PASSWORD-"));
            maintenance.Password.Add(Block.Source(SourceIds.Number));
            maintenance.Password.Add(Block.Text("-"));
            maintenance.Password.Add(Block.Source(SourceIds.Serial, SliceMode.Last, 2));
            maintenance.Notes.Add(Block.Text("Exemple fictif généré par la version publique de KPassPilot."));
            cfg.Models.Add(maintenance);

            var laptop = new PCType { Name = "LAPTOP", Description = "Portable de démonstration", IconId = 6 };
            laptop.Entries.Add(EntryRef.ToModel(maintenance.Id));
            var support = new EntryTemplate { Name = "Accès support", IconId = 0 };
            support.Title.Add(Block.Text("Support - "));
            support.Title.Add(Block.Source(SourceIds.PcName));
            support.Username.Add(Block.Source(SourceIds.Entity, SliceMode.All, 3, CaseMode.Lower));
            support.Username.Add(Block.Text("-support"));
            support.Password.Add(Block.Prop("DEMO-"));
            support.Password.Add(Block.Source(SourceIds.Number));
            support.Password.Add(Block.Prop("-EXAMPLE"));
            laptop.Entries.Add(EntryRef.Local(support));
            cfg.PCTypes.Add(laptop);

            var kiosk = new PCType { Name = "KIOSK", Description = "Borne de démonstration", IconId = 1 };
            kiosk.Entries.Add(EntryRef.ToModel(maintenance.Id));
            cfg.PCTypes.Add(kiosk);

            var server = new PCType { Name = "SERVER", Description = "Serveur de laboratoire fictif", IconId = 3 };
            server.Entries.Add(EntryRef.ToModel(maintenance.Id));
            cfg.PCTypes.Add(server);

            return cfg;
        }

        private void CreateMenu()
        {
            menu = new ToolStripMenuItem("KPassPilot");
            var create = new ToolStripMenuItem("Créer un équipement...");
            create.Click += (s, e) => ShowCreate();
            menu.DropDownItems.Add(create);

            var edit = new ToolStripMenuItem("Configuration...");
            edit.Click += (s, e) => ShowConfig();
            menu.DropDownItems.Add(edit);

            menu.DropDownItems.Add(new ToolStripSeparator());
            var about = new ToolStripMenuItem("À propos / Import-Export...");
            about.Click += (s, e) => ShowAbout();
            menu.DropDownItems.Add(about);
        }

        private void ShowCreate()
        {
            try { using (var form = new CreatePCForm(config, host)) form.ShowDialog(host.MainWindow); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "KPassPilot", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ShowConfig()
        {
            try { using (var form = new ConfigEditorForm(config, host, SaveConfiguration)) form.ShowDialog(host.MainWindow); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "KPassPilot", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ShowAbout()
        {
            try
            {
                using (var form = new AboutForm(config))
                    if (form.ShowDialog(host.MainWindow) == DialogResult.OK) SaveConfiguration();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "KPassPilot", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
