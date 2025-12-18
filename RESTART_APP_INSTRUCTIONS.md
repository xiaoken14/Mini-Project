# How to Restart Your Application

## Current Status

✅ **Your app is RUNNING at: http://localhost:5000**

The application is currently running (process ID 32560) but needs to be restarted to apply the database migration fixes.

## Steps to Restart

### Option 1: Stop and Restart (Recommended)

1. **Stop the current app:**
   - Press `Ctrl+C` in the terminal where the app is running
   - OR close the terminal window
   - OR use Task Manager to end the "HealthcareApp" process

2. **Start the app again:**
   ```bash
   dotnet run --project HealthcareApp.csproj
   ```

3. **Watch for migration messages:**
   You should see:
   ```
   Adding DoctorId column to AspNetUsers...
   ✓ DoctorId column added
   Adding PatientId column to AspNetUsers...
   ✓ PatientId column added
   ✓ Migration history record added
   ✓ Database migration applied successfully!
   ```

4. **Access your app:**
   Open browser to: **http://localhost:5000**

### Option 2: Kill Process and Restart

If you can't find the terminal:

```powershell
# Stop the process
Stop-Process -Name "HealthcareApp" -Force

# Wait a moment
Start-Sleep -Seconds 2

# Start the app
dotnet run --project HealthcareApp.csproj
```

## What Was Fixed

The code now includes automatic migration that will:
1. Add `DoctorId` column to `AspNetUsers` table
2. Add `PatientId` column to `AspNetUsers` table  
3. Record the migration in the database

This happens automatically when you restart the app!

## After Restart - Test the Doctor Schedule

1. **Login as doctor:**
   - Email: `abc123@gmail.com`
   - Password: `abc123`

2. **Navigate to Doctor Schedule:**
   - Go to `/Doctor/Schedule`
   - You should now see the weekly schedule page (not empty!)

3. **Create a schedule:**
   - Set times for Monday (e.g., 9:00 AM - 5:00 PM)
   - Click "Update Schedule"
   - You should see "Schedule updated successfully!"

4. **View daily schedule:**
   - Click "Daily View"
   - You should see time slots for today

5. **Test other features:**
   - Save template
   - Copy schedule to other days
   - View monthly calendar

## Troubleshooting

### If migration doesn't run automatically:

Run this SQL manually:

```sql
ALTER TABLE AspNetUsers ADD COLUMN DoctorId INTEGER NULL;
ALTER TABLE AspNetUsers ADD COLUMN PatientId INTEGER NULL;

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20251218025302_AddLegacyIdLinksToApplicationUser', '8.0.0');
```

### If you see "no such column: a.DoctorId" error:

The migration didn't apply. Try:
1. Stop the app completely
2. Delete `bin` and `obj` folders
3. Run `dotnet build HealthcareApp.csproj`
4. Run `dotnet run --project HealthcareApp.csproj`

### If schedules still don't show:

Check the database:
```sql
SELECT * FROM DoctorSchedules;
```

If empty, create a test schedule through the UI.

## Next Steps After Restart

Once the app is running with the migration applied:

1. **Link existing users** (if you have any):
   ```sql
   -- Link doctors by email
   UPDATE AspNetUsers 
   SET DoctorId = (SELECT Doctor_ID FROM Doctor WHERE Doctor.Email = AspNetUsers.Email)
   WHERE Email IN (SELECT Email FROM Doctor);
   
   -- Link patients by email
   UPDATE AspNetUsers 
   SET PatientId = (SELECT Patient_ID FROM Patient WHERE Patient.Email = AspNetUsers.Email)
   WHERE Email IN (SELECT Email FROM Patient);
   ```

2. **Update registration logic** to set DoctorId/PatientId when creating new users

3. **Test all schedule features** (see checklist in DOCTOR_SCHEDULE_FIX_SUMMARY.md)

## Quick Reference

- **App URL:** http://localhost:5000
- **Doctor Login:** abc123@gmail.com / abc123
- **Admin Login:** abc12345@gmail.com / abc12345
- **Schedule Page:** http://localhost:5000/Doctor/Schedule
- **Daily View:** http://localhost:5000/Doctor/DailyView
- **Monthly View:** http://localhost:5000/Doctor/MonthlyView
