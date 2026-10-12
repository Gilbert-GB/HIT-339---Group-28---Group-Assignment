using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library_System_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Library_System_Management.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AssignRoleModel : PageModel
{
    private static readonly HashSet<string> AllowedEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin@library.local",
        "reception@library.local",
        "manager@library.local"
    };
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AssignRoleModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<UserRow> Users { get; private set; } = new();
    public List<string> Roles { get; private set; } = new();

    [BindProperty]
    public string? SelectedUserId { get; set; }

    [BindProperty]
    public string? SelectedRole { get; set; }

    [BindProperty]
    public string? ActionType { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAsync();

        if (string.IsNullOrWhiteSpace(SelectedUserId) || string.IsNullOrWhiteSpace(SelectedRole))
        {
            ModelState.AddModelError(string.Empty, "Select a user and a role.");
            return Page();
        }

        var user = await _userManager.FindByIdAsync(SelectedUserId);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            return Page();
        }

        if (!await _roleManager.RoleExistsAsync(SelectedRole))
        {
            ModelState.AddModelError(string.Empty, "Role not found.");
            return Page();
        }

        var action = ActionType?.Trim().ToLowerInvariant();
        IdentityResult result;

        if (action == "remove")
        {
            result = await _userManager.RemoveFromRoleAsync(user, SelectedRole);
            StatusMessage = result.Succeeded
                ? $"Removed {SelectedRole} from {user.Email ?? user.UserName}."
                : string.Join(" ", result.Errors.Select(e => e.Description));
        }
        else
        {
            result = await _userManager.AddToRoleAsync(user, SelectedRole);
            StatusMessage = result.Succeeded
                ? $"Added {SelectedRole} to {user.Email ?? user.UserName}."
                : string.Join(" ", result.Errors.Select(e => e.Description));
        }

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Roles = await Task.FromResult(_roleManager.Roles.Select(r => r.Name).Where(n => !string.IsNullOrWhiteSpace(n)).OrderBy(n => n).Cast<string>().ToList());

        var users = _userManager.Users
            .Where(u => !string.IsNullOrWhiteSpace(u.Email) && AllowedEmails.Contains(u.Email))
            .OrderBy(u => u.Email)
            .ToList();
        var rows = new List<UserRow>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            rows.Add(new UserRow(
                user.Id,
                user.Email ?? user.UserName ?? user.Id,
                user.EmailConfirmed,
                userRoles.ToList()));
        }

        Users = rows;
    }

    public record UserRow(string Id, string DisplayName, bool EmailConfirmed, List<string> Roles);
}
