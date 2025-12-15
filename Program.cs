
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Mini_Project;
using Mini_Project.Data;

var builder = WebApplication.CreateBuilder(args);

// Define the path for the SQLite database file
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "MiniProject.db");

// Ensure the directory exists
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

// Configure the DbContext to use SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Helper class
builder.Services.AddScoped<Helper>();

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Add controllers with views
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Middleware for routing
app.UseRouting();

// Use Session
app.UseSession();

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
