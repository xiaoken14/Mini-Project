using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HealthcareApp.Data;
using HealthcareApp.Models;
using HealthcareApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure to use port 5000 (default)
builder.WebHost.UseUrls("http://localhost:5000");

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
    // Only use HTTPS redirection in production with proper HTTPS setup
    // app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// Disable HTTPS redirection for development to avoid warnings
// app.UseHttpsRedirection();
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
        
        // Apply pending migrations manually
        await ApplyMigrationManually(context);

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


async Task ApplyMigrationManually(ApplicationDbContext context)
{
    try
    {
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        
        // Check if DoctorId column exists
        command.CommandText = "PRAGMA table_info(AspNetUsers)";
        using var reader = await command.ExecuteReaderAsync();
        
        bool hasDoctorId = false;
        bool hasPatientId = false;
        
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(1);
            if (columnName == "DoctorId") hasDoctorId = true;
            if (columnName == "PatientId") hasPatientId = true;
        }
        reader.Close();
        
        // Add columns if they don't exist
        if (!hasDoctorId)
        {
            Console.WriteLine("Adding DoctorId column to AspNetUsers...");
            command.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN DoctorId INTEGER NULL";
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✓ DoctorId column added");
        }
        
        if (!hasPatientId)
        {
            Console.WriteLine("Adding PatientId column to AspNetUsers...");
            command.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN PatientId INTEGER NULL";
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✓ PatientId column added");
        }

        // Check if ConsultationFee column exists in Appointment table
        command.CommandText = "PRAGMA table_info(Appointment)";
        using var appointmentReader = await command.ExecuteReaderAsync();
        
        bool hasConsultationFee = false;
        while (await appointmentReader.ReadAsync())
        {
            var columnName = appointmentReader.GetString(1);
            if (columnName == "Consultation_Fee") hasConsultationFee = true;
        }
        appointmentReader.Close();

        if (!hasConsultationFee)
        {
            Console.WriteLine("Adding ConsultationFee column to Appointment...");
            command.CommandText = "ALTER TABLE Appointment ADD COLUMN Consultation_Fee DECIMAL(10,2) DEFAULT 300.00";
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("✓ ConsultationFee column added");
        }
        
        // Create __EFMigrationsHistory table if it doesn't exist
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (
                MigrationId TEXT NOT NULL PRIMARY KEY,
                ProductVersion TEXT NOT NULL
            )";
        await command.ExecuteNonQueryAsync();
        
        // Add migration history records if not exist
        var migrations = new[]
        {
            "20251218025302_AddLegacyIdLinksToApplicationUser",
            "20251218034607_AddConsultationFeeToAppointment"
        };

        foreach (var migrationId in migrations)
        {
            command.CommandText = $@"
                SELECT COUNT(*) FROM __EFMigrationsHistory 
                WHERE MigrationId = '{migrationId}'";
            var migrationExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
            
            if (!migrationExists)
            {
                command.CommandText = $@"
                    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                    VALUES ('{migrationId}', '8.0.0')";
                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"✓ Migration {migrationId} recorded");
            }
        }
        
        if (!hasDoctorId || !hasPatientId || !hasConsultationFee)
        {
            Console.WriteLine("✓ Database migration applied successfully!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not apply migration automatically: {ex.Message}");
        Console.WriteLine("Please run the migration manually using the SQL in DOCTOR_SCHEDULE_FIX_SUMMARY.md");
    }
}
