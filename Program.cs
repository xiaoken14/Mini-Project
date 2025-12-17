using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HealthcareApp.Data;
using HealthcareApp.Models;
using HealthcareApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Let .NET choose available ports automatically

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings - Make them less restrictive for simple passwords
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Enable email confirmation requirement
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

// Register custom services
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOTPService, OTPService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Seed database with roles and default users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        context.Database.EnsureCreated();

        // Create roles
        await CreateRoles(roleManager);

        // Create default users
        await CreateAdminUser(userManager);
        await CreateDoctorUser(userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Helper methods for seeding data
async Task CreateRoles(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Admin", "Doctor", "Patient" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

async Task CreateAdminUser(UserManager<ApplicationUser> userManager)
{
    string email = "abc12345@gmail.com";
    string password = "abc12345";

    var adminUser = await userManager.FindByEmailAsync(email);
    if (adminUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
            Console.WriteLine($"✅ Admin user created successfully: {email}");
        }
        else
        {
            Console.WriteLine($"❌ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}

async Task CreateDoctorUser(UserManager<ApplicationUser> userManager)
{
    string email = "abc123@gmail.com";
    string password = "abc123";

    var doctorUser = await userManager.FindByEmailAsync(email);
    if (doctorUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Dr. John",
            LastName = "Smith",
            Specialization = "General Practice"
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Doctor");
            Console.WriteLine($"✅ Doctor user created successfully: {email}");
        }
        else
        {
            Console.WriteLine($"❌ Failed to create doctor user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        // Update existing doctor with missing info
        if (string.IsNullOrEmpty(doctorUser.FirstName))
        {
            doctorUser.FirstName = "Dr. John";
            doctorUser.LastName = "Smith";
            doctorUser.Specialization = "General Practice";
            await userManager.UpdateAsync(doctorUser);
        }
    }
}