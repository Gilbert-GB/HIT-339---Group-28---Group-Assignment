using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library_System_Management.Controllers
{
    [Authorize(Roles = "Manager,Admin,Reception")]
    public class BranchesController : Controller
    {
        private readonly ILibraryRepository _repo;
        public BranchesController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            var branches = _repo.GetAllBranches();
            return View(branches);
        }

        [HttpPost]
        public IActionResult Transfer(string itemCode, System.Guid toBranchId)
        {
            if (string.IsNullOrWhiteSpace(itemCode)) return BadRequest("Item code required");
            var item = _repo.GetItemByCode(itemCode.Trim());
            if (item == null) return NotFound("Item not found");
            var ok = _repo.TransferItem(item.Id, toBranchId);
            if (!ok) TempData["TransferError"] = "Transfer failed. Check branch id.";
            else TempData["TransferSuccess"] = "Item transferred.";
            return RedirectToAction("Index");
        }
    }
}
