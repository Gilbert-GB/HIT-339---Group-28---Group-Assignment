using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Library_System_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    // ImportController: admin utility for bulk-importing items, either from a CSV
    // (file upload or pasted text) or from an external metadata provider.
    [Authorize(Roles = "Admin,Manager")]
    public class ImportController : Controller
    {
        private readonly ILibraryRepository _repo;
        private readonly IMetadataProvider _provider;

        public ImportController(ILibraryRepository repo, IMetadataProvider provider)
        {
            _repo = repo;
            _provider = provider;
        }

        // GET: /Import
        public IActionResult Index() => View(BuildModel());

        // POST: /Import/Upload
        // Accepts an uploaded .csv file, or pasted CSV text if no file is chosen.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1_048_576)] // 1 MB
        public async Task<IActionResult> Upload(IFormFile? csvFile, string? csvContent)
        {
            var model = BuildModel();
            string? csv = null;

            if (csvFile != null && csvFile.Length > 0)
            {
                if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    model.Error = "Please upload a file with a .csv extension.";
                    return View("Index", model);
                }
                using var reader = new StreamReader(csvFile.OpenReadStream());
                csv = await reader.ReadToEndAsync();
            }
            else if (!string.IsNullOrWhiteSpace(csvContent))
            {
                csv = csvContent;
            }

            if (csv == null)
            {
                model.Error = "Choose a CSV file or paste CSV content to import.";
                return View("Index", model);
            }

            model.Summary = new CsvItemImporter(_repo).Import(csv);
            return View("Index", model);
        }

        // GET: /Import/Lookup?query=...
        // Searches the external metadata provider by ISBN, title or artist.
        public IActionResult Lookup(string? query)
        {
            var model = BuildModel();
            model.Query = query;
            model.Results = _provider.Search(query ?? string.Empty).ToList();
            return View("Index", model);
        }

        // POST: /Import/ImportExternal
        // Imports one provider record as a new item at the chosen branch,
        // assigning the next free library code for its type.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ImportExternal(string externalId, Guid? branchId, string? query)
        {
            var model = BuildModel();
            model.Query = query;
            model.Results = _provider.Search(query ?? string.Empty).ToList();

            var record = _provider.GetById(externalId);
            if (record == null)
            {
                model.Error = "That record could not be found in the external catalogue.";
                return View("Index", model);
            }

            var existing = _repo.GetAllItems().FirstOrDefault(i =>
                i.GetType().Name == record.Type &&
                i.Name.Equals(record.Title, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                model.Error = $"'{record.Title}' is already in the catalogue as {existing.LibraryCode}.";
                return View("Index", model);
            }

            var branch = branchId.HasValue ? _repo.GetBranch(branchId.Value) : null;
            branch ??= model.Branches.FirstOrDefault(b => b.Name == "Central Library") ?? model.Branches.FirstOrDefault();

            Item item = record.Type == "Music"
                ? new Music { Artist = record.Creator, Year = record.Year ?? 0, Format = record.Category, LibraryCode = NextCode("M") }
                : new Book { Author = record.Creator, Genre = record.Category, LibraryCode = NextCode("B") };

            item.Name = record.Title;
            item.Status = ItemStatus.Available;
            item.BranchId = branch?.Id;
            _repo.AddItem(item);

            model.Message = $"Imported '{record.Title}' from {_provider.Name} as {item.LibraryCode} at {branch?.Name ?? "no branch"}.";
            return View("Index", model);
        }

        private ImportViewModel BuildModel() => new()
        {
            ProviderName = _provider.Name,
            Branches = _repo.GetAllBranches().ToList()
        };

        // NextCode: finds the highest existing numeric code for a prefix (e.g. B030)
        // and returns the next one (e.g. B031).
        private string NextCode(string prefix)
        {
            var max = _repo.GetAllItems()
                .Select(i => i.LibraryCode)
                .Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(c => int.TryParse(c.Substring(prefix.Length), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();
            return $"{prefix}{max + 1:000}";
        }
    }
}