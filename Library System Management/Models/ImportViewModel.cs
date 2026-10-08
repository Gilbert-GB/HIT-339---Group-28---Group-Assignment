using Library_System_Management.Services;

namespace Library_System_Management.Models
{
    // ImportViewModel: everything the Import page needs, including CSV results
    // and external provider search results.
    public class ImportViewModel
    {
        public ImportSummary? Summary { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
        public string? Query { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public List<ExternalItemRecord> Results { get; set; } = new();
        public List<Branch> Branches { get; set; } = new();
    }
}