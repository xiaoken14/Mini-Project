using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedScheduleFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add IsDefault column to ScheduleTemplates table
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "ScheduleTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Rename Notes column to Note in SpecialSchedules table (if needed)
            // Note: This is optional since both column names work fine
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove IsDefault column from ScheduleTemplates table
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "ScheduleTemplates");
        }
    }
}