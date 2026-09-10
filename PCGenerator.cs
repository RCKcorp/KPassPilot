using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KPassPilot
{
    /// <summary>
    /// Moteur public de démonstration. La convention utilisée ici est volontairement fictive
    /// et différente de toute convention pouvant être utilisée dans un environnement réel.
    /// </summary>
    public static class PCGenerator
    {
        // Exemple public : NORTH.LAPTOP.TEST.4827
        private const string PCNamePattern = @"^([A-Z0-9]{2,12})\.([A-Z0-9]{2,12})\.([A-Z0-9]{2,12})\.([A-Z0-9]{2,8})$";

        public static PCNameParsed TryParse(string pcName)
        {
            if (string.IsNullOrEmpty(pcName)) return null;
            var m = Regex.Match(pcName.Trim().ToUpperInvariant(), PCNamePattern);
            if (!m.Success) return null;

            return new PCNameParsed
            {
                Zone = m.Groups[1].Value,
                Category = m.Groups[2].Value,
                Environment = m.Groups[3].Value,
                AssetId = m.Groups[4].Value,
                FullName = m.Value
            };
        }

        public static PCNameParsed ParsePCName(string pcName)
        {
            var parsed = TryParse(pcName);
            if (parsed == null)
                throw new FormatException("Format invalide : " + pcName + ". Exemple de démo attendu : NORTH.LAPTOP.TEST.4827.");
            return parsed;
        }

        public static bool ValidateSN(string sn) { return !string.IsNullOrEmpty(sn) && sn.Trim().Length > 0; }

        public static string SourceValue(string src, PCNameParsed p, string sn)
        {
            if (p == null) return string.Empty;
            switch (src)
            {
                case SourceIds.PcName: return p.FullName ?? string.Empty;
                case SourceIds.Entity: return p.Zone ?? string.Empty;
                case SourceIds.Type: return p.Category ?? string.Empty;
                case SourceIds.System: return p.Environment ?? string.Empty;
                case SourceIds.Number: return p.AssetId ?? string.Empty;
                case SourceIds.Serial: return sn ?? string.Empty;
                default: return string.Empty;
            }
        }

        public static string BlockValue(Block block, PCNameParsed parsed, string sn, PCType type)
        {
            if (block == null) return string.Empty;
            string value;
            if (block.Kind == BlockKind.Text) value = block.Value ?? string.Empty;
            else if (block.Kind == BlockKind.Prop) value = block.Key ?? string.Empty;
            else
            {
                value = SourceValue(block.Src, parsed, sn);
                value = Cut(value, block);
            }

            if (block.Case == CaseMode.Upper) value = value.ToUpperInvariant();
            else if (block.Case == CaseMode.Lower) value = value.ToLowerInvariant();
            return value;
        }

        private static string Cut(string value, Block block)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            int n = Math.Max(1, block.N);
            switch (block.Mode)
            {
                case SliceMode.First:
                    return value.Substring(0, Math.Min(n, value.Length));
                case SliceMode.Last:
                    return n >= value.Length ? value : value.Substring(value.Length - n);
                case SliceMode.Slice:
                    int start = Math.Max(0, block.From - 1);
                    if (start >= value.Length) return string.Empty;
                    int len = Math.Min(Math.Max(1, block.Len), value.Length - start);
                    return value.Substring(start, len);
                default:
                    return value;
            }
        }

        public static string FieldValue(List<Block> blocks, PCNameParsed parsed, string sn, PCType type)
        {
            if (blocks == null || blocks.Count == 0) return string.Empty;
            var sb = new StringBuilder();
            foreach (var block in blocks) sb.Append(BlockValue(block, parsed, sn, type));
            return sb.ToString();
        }

        public static GeneratedEntry Generate(EntryTemplate entry, PCNameParsed parsed, string sn, PCType type)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            if (parsed == null) throw new ArgumentNullException("parsed");
            return new GeneratedEntry
            {
                Title = FieldValue(entry.Title, parsed, sn, type),
                Username = FieldValue(entry.Username, parsed, sn, type),
                Password = FieldValue(entry.Password, parsed, sn, type),
                Notes = FieldValue(entry.Notes, parsed, sn, type),
                IconId = entry.IconId
            };
        }

        // Dans la version publique, les valeurs globales sont des fragments littéraux sélectionnés
        // dans l'éditeur. Il n'existe donc pas de propriété métier obligatoire propre à un profil.
        public static List<string> MissingProps(EntryTemplate entry, PCType type) { return new List<string>(); }

        public static string BlockLabel(Block block, KPassPilotConfig config)
        {
            if (block == null) return string.Empty;
            if (block.Kind == BlockKind.Text) return string.Empty;

            string name = block.Kind == BlockKind.Prop ? (block.Key ?? "Valeur globale") : SourceIds.Label(block.Src);
            var sb = new StringBuilder(name);
            if (block.Kind == BlockKind.Source)
            {
                if (block.Mode == SliceMode.First) sb.Append(" · ").Append(block.N).Append(" prem.");
                else if (block.Mode == SliceMode.Last) sb.Append(" · ").Append(block.N).Append(" dern.");
                else if (block.Mode == SliceMode.Slice) sb.Append(" · car.").Append(block.From).Append("→").Append(block.From + block.Len - 1);
            }
            if (block.Case == CaseMode.Upper) sb.Append(" · MAJ");
            else if (block.Case == CaseMode.Lower) sb.Append(" · min");
            return sb.ToString();
        }
    }
}
