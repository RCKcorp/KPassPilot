using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace KPassPilot
{
    public enum BlockKind { Text, Source, Prop }
    public enum SliceMode { All, First, Last, Slice }
    public enum CaseMode { None, Upper, Lower }

    /// <summary>Données publiques de démonstration extraites d'un identifiant d'équipement fictif.</summary>
    public static class SourceIds
    {
        public const string PcName = "asset-name";
        public const string Entity = "zone";
        public const string Type = "category";
        public const string System = "environment";
        public const string Number = "asset-id";
        public const string Serial = "serial";

        public static readonly string[] All = { PcName, Entity, Type, System, Number, Serial };

        public static string Label(string id)
        {
            switch (id)
            {
                case PcName: return "Identifiant complet";
                case Entity: return "Zone";
                case Type: return "Catégorie";
                case System: return "Environnement";
                case Number: return "Identifiant équipement";
                case Serial: return "N° de série";
                default: return id;
            }
        }
    }

    [Serializable]
    public class Block
    {
        [XmlAttribute] public BlockKind Kind { get; set; }
        [XmlAttribute] public string Value { get; set; }
        [XmlAttribute] public string Src { get; set; }
        [XmlAttribute] public SliceMode Mode { get; set; }
        [XmlAttribute] public int N { get; set; }
        [XmlAttribute] public int From { get; set; }
        [XmlAttribute] public int Len { get; set; }
        [XmlAttribute] public string Key { get; set; }
        [XmlAttribute] public CaseMode Case { get; set; }

        public Block()
        {
            Kind = BlockKind.Text;
            Value = string.Empty;
            Src = SourceIds.Entity;
            Mode = SliceMode.All;
            N = 3;
            From = 1;
            Len = 2;
            Key = string.Empty;
            Case = CaseMode.None;
        }

        public static Block Text(string value) { return new Block { Kind = BlockKind.Text, Value = value }; }
        public static Block Source(string src, SliceMode mode = SliceMode.All, int n = 3, CaseMode c = CaseMode.None)
        {
            return new Block { Kind = BlockKind.Source, Src = src, Mode = mode, N = n, Case = c };
        }
        public static Block Prop(string value, CaseMode c = CaseMode.None)
        {
            return new Block { Kind = BlockKind.Prop, Key = value, Case = c };
        }
        public Block Clone() { return (Block)MemberwiseClone(); }
    }

    [Serializable]
    public class EntryTemplate
    {
        private List<Block> title = new List<Block>();
        private List<Block> username = new List<Block>();
        private List<Block> password = new List<Block>();
        private List<Block> notes = new List<Block>();

        [XmlAttribute] public string Id { get; set; }
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public int IconId { get; set; }

        [XmlArray("Title"), XmlArrayItem("Block")]
        public List<Block> Title { get { return title; } set { title = value ?? new List<Block>(); } }
        [XmlArray("Username"), XmlArrayItem("Block")]
        public List<Block> Username { get { return username; } set { username = value ?? new List<Block>(); } }
        [XmlArray("Password"), XmlArrayItem("Block")]
        public List<Block> Password { get { return password; } set { password = value ?? new List<Block>(); } }
        [XmlArray("Notes"), XmlArrayItem("Block")]
        public List<Block> Notes { get { return notes; } set { notes = value ?? new List<Block>(); } }

        public List<Block> Field(string field)
        {
            switch (field)
            {
                case "title": return Title;
                case "user": return Username;
                case "pass": return Password;
                case "notes": return Notes;
                default: throw new ArgumentException("Champ inconnu : " + field);
            }
        }

        public EntryTemplate Clone()
        {
            var copy = new EntryTemplate { Id = Id, Name = Name, IconId = IconId };
            foreach (var b in Title) copy.Title.Add(b.Clone());
            foreach (var b in Username) copy.Username.Add(b.Clone());
            foreach (var b in Password) copy.Password.Add(b.Clone());
            foreach (var b in Notes) copy.Notes.Add(b.Clone());
            return copy;
        }
    }

    [Serializable]
    public class EntryRef
    {
        [XmlAttribute] public string Ref { get; set; }
        [XmlElement("Entry")] public EntryTemplate Inline { get; set; }
        public static EntryRef ToModel(string id) { return new EntryRef { Ref = id }; }
        public static EntryRef Local(EntryTemplate entry) { return new EntryRef { Inline = entry }; }
        [XmlIgnore] public bool IsShared { get { return !string.IsNullOrEmpty(Ref); } }
    }

    [Serializable]
    public class PCType
    {
        private List<EntryRef> entries = new List<EntryRef>();
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public string Description { get; set; }
        [XmlAttribute] public int IconId { get; set; }
        [XmlArray("Entries"), XmlArrayItem("Entry")]
        public List<EntryRef> Entries { get { return entries; } set { entries = value ?? new List<EntryRef>(); } }
    }

    [Serializable]
    [XmlRoot("KPassPilotConfig")]
    public class KPassPilotConfig
    {
        private List<string> globalValues = new List<string>();
        private List<EntryTemplate> models = new List<EntryTemplate>();
        private List<PCType> pcTypes = new List<PCType>();

        [XmlArray("GlobalValues"), XmlArrayItem("Value")]
        public List<string> GlobalValues { get { return globalValues; } set { globalValues = value ?? new List<string>(); } }
        [XmlArray("Models"), XmlArrayItem("Model")]
        public List<EntryTemplate> Models { get { return models; } set { models = value ?? new List<EntryTemplate>(); } }
        [XmlArray("PCTypes"), XmlArrayItem("PCType")]
        public List<PCType> PCTypes { get { return pcTypes; } set { pcTypes = value ?? new List<PCType>(); } }

        public EntryTemplate ModelById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return Models.Find(m => m.Id == id);
        }

        public EntryTemplate Resolve(EntryRef entry)
        {
            if (entry == null) return null;
            return entry.IsShared ? ModelById(entry.Ref) : entry.Inline;
        }

        public List<PCType> TypesUsing(string modelId)
        {
            return PCTypes.FindAll(t => t.Entries.Exists(e => e.Ref == modelId));
        }

        public string PropName(string key) { return key ?? string.Empty; }
    }

    /// <summary>Identifiant public fictif : ZONE.CATEGORY.ENV.ID.</summary>
    public class PCNameParsed
    {
        public string Zone { get; set; }
        public string Category { get; set; }
        public string Environment { get; set; }
        public string AssetId { get; set; }
        public string FullName { get; set; }
    }

    public class GeneratedEntry
    {
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Notes { get; set; }
        public int IconId { get; set; }
    }
}
