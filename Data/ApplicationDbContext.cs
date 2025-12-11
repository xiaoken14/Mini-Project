// filepath: c:\Users\wwxia\Downloads\Mini-Project\Data\ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mini_Project.Models;

namespace Mini_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Appointment -> Patient (FK: Patient_ID)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.Patient_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment -> Doctor (FK: Doctor_ID)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.Doctor_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> Patient (FK: Patient_ID)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.Patient_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> Appointment (FK: Appointment_ID)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Appointment)
                .WithMany()
                .HasForeignKey(p => p.Appointment_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Report -> Patient (FK: Patient_ID)
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Patient)
                .WithMany()
                .HasForeignKey(r => r.Patient_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Report -> Doctor (FK: Doctor_ID)
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Doctor)
                .WithMany()
                .HasForeignKey(r => r.Doctor_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor -> Admin (FK: Admin_ID)
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Admin)
                .WithMany()
                .HasForeignKey(d => d.Admin_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient -> Admin (FK: Admin_ID)
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Admin)
                .WithMany()
                .HasForeignKey(p => p.Admin_ID)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<Payment> Payments { get; set; }
    }

// Configure relationships explicitly to avoid EF creating shadow foreign keys
}