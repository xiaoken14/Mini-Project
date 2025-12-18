# Doctor Schedule System - Fix Summary

## Problem Identified

Your application has **two separate user/doctor systems** that weren't properly connected:

1. **ApplicationUser** (Identity-based) - Uses `string Id` (GUID)
   - Used by: `DoctorSchedule`, `SpecialSchedule`, `ScheduleTemplate`
   - Managed by: ASP.NET Identity

2. **Doctor** table - Uses `int DoctorId`
   - Used by: `Appointment`, `Notification`, `Payment`
   - Legacy database schema

The `DoctorController` was trying to bridge these systems but had placeholder code that returned null/empty data.

## Fixes Applied

### 1. Updated Models/User.cs
Added linking properties to connect the two systems:
```csharp
public int? DoctorId { get; set; }  // Link to legacy Doctor table
public int? PatientId { get; set; } // Link to legacy Patient table
```

### 2. Fixed Controllers/DoctorController.cs
**All methods now properly:**
- Query schedules using `ApplicationUser.Id` (string)
- Query appointments using `ApplicationUser.DoctorId` (int) when available
- Handle authentication checks
- Implement proper error handling

**Key fixes:**
- `Schedule()` - Now loads actual schedules from database
- `UpdateSchedule()` - Properly creates/updates schedules
- `DailyView()` - Loads schedules and appointments correctly
- `DeleteSchedule()` - Actually deletes schedules
- `CopySchedule()` - Properly copies schedules between days
- `SaveTemplate()` - Added validation
- `GetTemplates()` - Added error handling
- `ClearAllSchedules()` - Added authentication check

### 3. Fixed Services/ScheduleService.cs
- `GetMonthlyScheduleAsync()` - Now retrieves appointments using the DoctorId link
- `SaveScheduleTemplateAsync()` - Fixed to use JSON serialization workaround
- `ApplyScheduleTemplateAsync()` - Fixed to deserialize and apply templates
- `GetScheduleTemplatesAsync()` - Cleans up template descriptions for display

### 4. Updated Models/DoctorViewModels.cs
Added missing properties to `DailyScheduleViewModel`:
```csharp
public List<DoctorSchedule> WeeklySchedules { get; set; } = new();
public string DoctorName { get; set; } = string.Empty;
```

### 5. Created Migration
File: `Migrations/20251218025302_AddLegacyIdLinksToApplicationUser.cs`
- Adds `DoctorId` column to `AspNetUsers` table
- Adds `PatientId` column to `AspNetUsers` table

## Manual Steps Required

### Step 1: Apply the Database Migration

Due to EF tools issues, you need to manually apply the migration:

**Option A: Using SQL directly**
```sql
ALTER TABLE AspNetUsers ADD COLUMN DoctorId INTEGER NULL;
ALTER TABLE AspNetUsers ADD COLUMN PatientId INTEGER NULL;
```

**Option B: Fix EF tools and run migration**
```bash
# Try updating EF tools
dotnet tool update --global dotnet-ef

# Then run
dotnet ef database update --project HealthcareApp.csproj
```

### Step 2: Link Existing Users to Legacy Tables

After the migration, you need to link existing ApplicationUser records to their corresponding Doctor/Patient records.

**For Doctors:**
```sql
-- Match by email
UPDATE AspNetUsers 
SET DoctorId = (
    SELECT Doctor_ID 
    FROM Doctor 
    WHERE Doctor.Email = AspNetUsers.Email
)
WHERE AspNetUsers.Email IN (SELECT Email FROM Doctor);
```

**For Patients:**
```sql
-- Match by email
UPDATE AspNetUsers 
SET PatientId = (
    SELECT Patient_ID 
    FROM Patient 
    WHERE Patient.Email = AspNetUsers.Email
)
WHERE AspNetUsers.Email IN (SELECT Email FROM Patient);
```

### Step 3: Update Registration/Login Logic

Ensure that when new doctors/patients register:
1. Create record in `Doctor`/`Patient` table
2. Create `ApplicationUser` record
3. **Link them by setting `ApplicationUser.DoctorId` or `ApplicationUser.PatientId`**

Example in AccountController:
```csharp
// After creating doctor in Doctor table
var doctor = new Doctor { /* ... */ };
await _context.Doctors.AddAsync(doctor);
await _context.SaveChangesAsync();

// Create ApplicationUser and link it
var user = new ApplicationUser 
{ 
    /* ... */,
    DoctorId = doctor.DoctorId  // Link here!
};
await _userManager.CreateAsync(user, password);
```

## How It Works Now

### Schedule Management Flow
1. Doctor logs in → `ApplicationUser.Id` (GUID string) is used
2. Doctor creates schedule → Stored with `DoctorSchedule.DoctorId = ApplicationUser.Id`
3. Doctor views appointments → System looks up `ApplicationUser.DoctorId` (int) to query `Appointment` table

### Data Retrieval
```csharp
// Get schedules (uses ApplicationUser.Id)
var schedules = await _context.DoctorSchedules
    .Where(s => s.DoctorId == applicationUserId)
    .ToListAsync();

// Get appointments (uses ApplicationUser.DoctorId → Doctor.DoctorId)
var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == applicationUserId);
if (appUser?.DoctorId != null)
{
    var appointments = await _context.Appointments
        .Where(a => a.DoctorId == appUser.DoctorId.Value)
        .ToListAsync();
}
```

## Testing Checklist

After applying the migration and linking users:

- [ ] Doctor can view weekly schedule page
- [ ] Doctor can create/update schedules for each day
- [ ] Doctor can view daily schedule with time slots
- [ ] Doctor can see appointments in daily view
- [ ] Doctor can view monthly calendar
- [ ] Doctor can save schedule templates
- [ ] Doctor can load and apply templates
- [ ] Doctor can copy schedules between days
- [ ] Doctor can delete schedules
- [ ] Doctor can create special schedules
- [ ] Clear All button works

## Files Modified

1. `Models/User.cs` - Added DoctorId and PatientId properties
2. `Controllers/DoctorController.cs` - Complete rewrite with proper database queries
3. `Services/ScheduleService.cs` - Fixed appointment retrieval and template management
4. `Models/DoctorViewModels.cs` - Added missing properties
5. `Migrations/20251218025302_AddLegacyIdLinksToApplicationUser.cs` - New migration

## Important Notes

- The system now properly bridges the two user systems
- All schedule operations use `ApplicationUser.Id` (string GUID)
- All appointment operations use `ApplicationUser.DoctorId` (int)
- Templates are stored as JSON in the Description field (workaround)
- Error handling is in place for all operations
- Authentication checks are added to all actions

## Next Steps

1. Apply the database migration (see Step 1 above)
2. Link existing users (see Step 2 above)
3. Update registration logic (see Step 3 above)
4. Test all functionality (see Testing Checklist)
5. Consider migrating to a single user system in the future for cleaner architecture
