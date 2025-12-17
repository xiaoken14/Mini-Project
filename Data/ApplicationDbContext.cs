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

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<SpecialSchedule> SpecialSchedules { get; set; }
        public DbSet<ScheduleTemplate> ScheduleTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure ApplicationUser entity
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.LicenseNumber).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            });

            // Configure Appointment entity
            builder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Patient)
                    .WithMany()
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Doctor)
                    .WithMany()
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
            });

            // Configure DoctorSchedule entity
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
                
                // Ensure unique schedule per doctor per day
                entity.HasIndex(e => new { e.DoctorId, e.DayOfWeek }).IsUnique();
            });

            // Configure SpecialSchedule entity
            builder.Entity<SpecialSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Doctor)
                    .WithMany()
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(e => e.Note).HasMaxLength(500);
                
                // Ensure unique special schedule per doctor per date
                entity.HasIndex(e => new { e.DoctorId, e.Date }).IsUnique();
            });

            // Configure ScheduleTemplate entity
            builder.Entity<ScheduleTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });
        }
    }
}