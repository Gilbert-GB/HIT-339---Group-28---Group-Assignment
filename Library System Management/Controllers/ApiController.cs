using Library_System_Management.Models;
using Library_System_Management.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Library_System_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILibraryRepository _repo;
        private readonly IConfiguration _config;

        public ApiController(ILibraryRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        // simple API key check using header X-Api-Key
        private bool IsAuthorized()
        {
            var expected = _config["ApiKey"];
            if (string.IsNullOrWhiteSpace(expected)) return false;
            if (!Request.Headers.TryGetValue("X-Api-Key", out var provided)) return false;
            return string.Equals(provided.FirstOrDefault(), expected, StringComparison.Ordinal);
        }

        // GET: api/Api/available (requires API key)
        [HttpGet("available")]
        public IActionResult GetAvailable()
        {
            if (!IsAuthorized()) return Unauthorized();
            var items = _repo.GetAllItems().Where(i => i.Status == ItemStatus.Available).Select(i => new {
                i.Id,
                i.LibraryCode,
                i.Name,
                Type = i.GetType().Name,
                BranchId = i.BranchId
            });
            return Ok(items);
        }

        // GET: api/Api/categories (requires API key)
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            if (!IsAuthorized()) return Unauthorized();
            var types = _repo.GetAllItems().Select(i => i.GetType().Name).Distinct();
            return Ok(new { types });
        }

        // GET: api/Api/status (public)
        // Operating status is not sensitive, so this endpoint needs no API key. This also lets the
        // "API Status" link in the site navigation open it directly in a browser, which cannot send
        // the X-Api-Key header. Catalogue data endpoints above remain key-protected.
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            // simple operating hours: 09:00 - 17:00 local time
            var now = DateTime.Now;
            var open = now.Hour >= 9 && now.Hour < 17;
            var branches = _repo.GetAllBranches().Select(b => new { b.Id, b.Name, b.Address });
            return Ok(new { open, serverTime = now, branches });
        }
    }
}