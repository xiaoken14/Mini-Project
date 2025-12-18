using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareApp.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationFeeToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Consultation_Fee",
                table: "Appointment",
                type: "DECIMAL(10,2)",
                nullable: true,
                defaultValue: 300.00m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Consultation_Fee",
                table: "Appointment");
        }
    }
}