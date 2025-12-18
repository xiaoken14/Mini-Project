-- Add DoctorId and PatientId columns to AspNetUsers table
ALTER TABLE AspNetUsers ADD COLUMN DoctorId INTEGER NULL;
ALTER TABLE AspNetUsers ADD COLUMN PatientId INTEGER NULL;

-- Insert migration history record
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20251218025302_AddLegacyIdLinksToApplicationUser', '8.0.0');
