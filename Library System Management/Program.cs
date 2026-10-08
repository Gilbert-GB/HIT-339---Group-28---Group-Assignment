// Program.cs: application startup and service configuration for the web app.
// Sets up DbContext, Identity, DI services, middleware and request pipeline.
using Library_System_Management.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Library_System_Management.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
// register in-memory repository for demo purposes
builder.Services.AddSingleton<ILibraryRepository, InMemoryLibraryRepository>();

var app = builder.Build();

// Apply any pending migrations automatically, so the Identity database is created
// the first time the application runs on a new machine (no manual Update-Database step).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Seed default roles and demo users (Admin, Reception, Manager)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        string[] roles = new[] { "Admin", "Reception", "Manager" };
        foreach (var r in roles)
        {
            var exists = await roleManager.RoleExistsAsync(r);
            if (!exists)
            {
                await roleManager.CreateAsync(new IdentityRole(r));
            }
        }

        async Task EnsureUser(string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var res = await userManager.CreateAsync(user, password);
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            else
            {
                var inRole = await userManager.IsInRoleAsync(user, role);
                if (!inRole) await userManager.AddToRoleAsync(user, role);
            }
        }

        // demo credentials - change for production
        await EnsureUser("admin@library.local", "P@ssw0rd1!", "Admin");
        await EnsureUser("reception@library.local", "P@ssw0rd1!", "Reception");
        await EnsureUser("manager@library.local", "P@ssw0rd1!", "Manager");
    }
    catch (Exception ex)
    {
        // can't throw on startup - log and continue
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Error seeding users and roles");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();