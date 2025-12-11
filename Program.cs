using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mini_Project.Data;
using Mini_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// Define the path for the SQLite database file
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "MiniProject.db");

// Ensure the directory exists
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

// Configure the DbContext to use SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Add Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Enable email confirmation
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add controllers with views
builder.Services.AddControllersWithViews();

// Email Sender - simple dev implementation
builder.Services.AddSingleton<IEmailSender>(new EmailSender("localhost", 25, "noreply@example.com", "password"));

var app = builder.Build();

// Middleware for routing
app.UseRouting();

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();