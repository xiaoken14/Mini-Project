using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HealthcareApp.Models;

namespace HealthcareApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // New DbSets matching your database schema
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // Keep existing schedule-related DbSets if still needed
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<SpecialSchedule> SpecialSchedules { get; set; }
        public DbSet<ScheduleTemplate> ScheduleTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure ApplicationUser entity (keep existing for Identity)
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.LicenseNumber).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            });

            // Configure Admin entity
            builder.Entity<Admin>(entity =>
            {
                entity.HasKey(e => e.AdminId);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContactNo).HasMaxLength(20);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Doctor entity
            builder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.DoctorId);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.ConsultationHours).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.HasOne(e => e.Admin)
                    .WithMany(a => a.Doctors)
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Patient entity
            builder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.PatientId);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContactNo).HasMaxLength(20);
                entity.Property(e => e.Category).HasMaxLength(20);
                entity.Property(e => e.DiscountEligibility).HasMaxLength(10).HasDefaultValue("No");
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.HasOne(e => e.Admin)
                    .WithMany(a => a.Patients)
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Appointment entity
            builder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.AppointmentId);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Booked");
                entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("Normal");
                entity.Property(e => e.DiscountApplied).HasMaxLength(10).HasDefaultValue("No");
                
                entity.HasOne(e => e.Admin)
                    .WithMany(a => a.Appointments)
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Notification entity
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.Type).HasMaxLength(30);
                
                entity.HasOne(e => e.Appointment)
                    .WithMany(a => a.Notifications)
                    .HasForeignKey(e => e.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Report entity
            builder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.ReportId);
                entity.Property(e => e.ReportType).HasMaxLength(50);
                entity.Property(e => e.FilePath).HasMaxLength(255);
                
                entity.HasOne(e => e.Admin)
                    .WithMany(a => a.Reports)
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Payment entity
            builder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PaymentStatus).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.PaymentMethod).HasMaxLength(20);
                
                entity.HasOne(e => e.Appointment)
                    .WithMany(a => a.Payments)
                    .HasForeignKey(e => e.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Keep existing schedule configurations if still needed
            builder.Entity<DoctorSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Doctor)
                    .WithMany()
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.ScheduleTemplate)
                    .WithMany(t => t.WeeklySchedules)
                    .HasForeignKey(e => e.ScheduleTemplateId);
                
                entity.HasIndex(e => new { e.DoctorId, e.DayOfWeek }).IsUnique();
            });

            builder.Entity<SpecialSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Doctor)
                    .WithMany()
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(e => e.Note).HasMaxLength(500);
                entity.HasIndex(e => new { e.DoctorId, e.Date }).IsUnique();
            });

            builder.Entity<ScheduleTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });
        }
    }
}