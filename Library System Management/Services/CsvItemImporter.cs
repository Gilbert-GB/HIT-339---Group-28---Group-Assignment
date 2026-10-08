using System.Globalization;
using System.Text;
using Library_System_Management.Models;
using Library_System_Management.Repositories;

namespace Library_System_Management.Services
{
    // ImportSummary: result of a CSV import, with a line-by-line list of rejected rows.
    public class ImportSummary
    {
        public int Imported { get; set; }
        public int Failed => Errors.Count;
        public List<string> Errors { get; } = new();
    }

    // CsvItemImporter: validates and imports catalogue items from CSV text.
    // Columns are matched by header name (case-insensitive), so column order does not matter
    // and unknown columns are ignored.
    //   Required: LibraryCode, Type, Name
    //   Optional: Branch, Author, Genre, Pages (books), Artist, Year, Format (music),
    //             ToyType, RecommendedAge (toys)
    public class CsvItemImporter
    {
        private readonly ILibraryRepository _repo;

        public CsvItemImporter(ILibraryRepository repo)
        {
            _repo = repo;
        }

        public ImportSummary Import(string csv)
        {
            var summary = new ImportSummary();

            var lines = csv.TrimStart('\uFEFF')
                .Replace("\r\n", "\n").Replace('\r', '\n')
                .Split('\n')
                .Select(l => l.Trim())
                .ToList();

            var headerIndex = lines.FindIndex(l => l.Length > 0);
            if (headerIndex < 0)
            {
                summary.Errors.Add("The CSV is empty.");
                return summary;
            }

            var header = ParseLine(lines[headerIndex]).Select(h => h.Trim()).ToList();
            int Col(string name) => header.FindIndex(h => h.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (Col("LibraryCode") < 0 || Col("Type") < 0 || Col("Name") < 0)
            {
                summary.Errors.Add("The header row must include LibraryCode, Type and Name columns.");
                return summary;
            }

            var branches = _repo.GetAllBranches().ToList();
            var defaultBranch = branches.FirstOrDefault(b => b.Name == "Central Library") ?? branches.FirstOrDefault();
            var codesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = headerIndex + 1; i < lines.Count; i++)
            {
                if (lines[i].Length == 0) continue;

                var lineNo = i + 1; // line number as shown in a text editor
                var fields = ParseLine(lines[i]);

                string Get(string column)
                {
                    var c = Col(column);
                    return c >= 0 && c < fields.Count ? fields[c].Trim() : string.Empty;
                }

                bool TryInt(string column, out int value)
                {
                    var raw = Get(column);
                    value = 0;
                    return raw.Length == 0 || int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
                }

                var code = Get("LibraryCode").ToUpperInvariant();
                var typeRaw = Get("Type");
                var type = typeRaw.ToLowerInvariant();
                var name = Get("Name");

                if (code.Length == 0 || name.Length == 0)
                {
                    summary.Errors.Add($"Line {lineNo}: LibraryCode and Name are required.");
                    continue;
                }

                var expectedPrefix = type switch { "book" => "B", "music" => "M", "toy" => "T", _ => null };
                if (expectedPrefix == null)
                {
                    summary.Errors.Add($"Line {lineNo}: unknown Type '{typeRaw}'. Use Book, Music or Toy.");
                    continue;
                }
                if (!code.StartsWith(expectedPrefix))
                {
                    summary.Errors.Add($"Line {lineNo}: code '{code}' should start with '{expectedPrefix}' for a {typeRaw}.");
                    continue;
                }
                if (_repo.GetItemByCode(code) != null || !codesInFile.Add(code))
                {
                    summary.Errors.Add($"Line {lineNo}: code '{code}' already exists.");
                    continue;
                }

                var branch = defaultBranch;
                var branchName = Get("Branch");
                if (branchName.Length > 0)
                {
                    branch = branches.FirstOrDefault(b => b.Name.Equals(branchName, StringComparison.OrdinalIgnoreCase));
                    if (branch == null)
                    {
                        summary.Errors.Add($"Line {lineNo}: unknown branch '{branchName}'.");
                        continue;
                    }
                }

                Item item;
                if (type == "book")
                {
                    if (!TryInt("Pages", out var pages))
                    {
                        summary.Errors.Add($"Line {lineNo}: Pages must be a whole number.");
                        continue;
                    }
                    item = new Book { Author = Get("Author"), Genre = Get("Genre"), Pages = pages };
                }
                else if (type == "music")
                {
                    if (!TryInt("Year", out var year))
                    {
                        summary.Errors.Add($"Line {lineNo}: Year must be a whole number.");
                        continue;
                    }
                    item = new Music { Artist = Get("Artist"), Year = year, Format = Get("Format") };
                }
                else
                {
                    if (!TryInt("RecommendedAge", out var age))
                    {
                        summary.Errors.Add($"Line {lineNo}: RecommendedAge must be a whole number.");
                        continue;
                    }
                    item = new Toy { ToyType = Get("ToyType"), RecommendedAge = age };
                }

                item.LibraryCode = code;
                item.Name = name;
                item.Status = ItemStatus.Available;
                item.BranchId = branch?.Id;

                _repo.AddItem(item);
                summary.Imported++;
            }

            return summary;
        }

        // ParseLine: splits one CSV line into fields, supporting quoted values that
        // contain commas and doubled quotes ("") inside quoted values.
        private static List<string> ParseLine(string line)
        {
            var result = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(c);
                    }
                }
                else if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            result.Add(field.ToString());
            return result;
        }
    }
}