using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ImportController : Controller
    {
        private readonly ILibraryRepository _repo;
        public ImportController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Upload(string csvContent)
        {
            if (string.IsNullOrWhiteSpace(csvContent))
            {
                TempData["ImportError"] = "CSV content required.";
                return RedirectToAction("Index");
            }
            var (success, failed) = _repo.ImportItemsFromCsv(csvContent);
            TempData["ImportResult"] = $"Imported: {success}, Failed: {failed}";
            return RedirectToAction("Index");
        }
    }
}
