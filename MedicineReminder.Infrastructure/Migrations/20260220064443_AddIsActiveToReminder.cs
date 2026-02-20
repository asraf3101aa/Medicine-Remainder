using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicineReminder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Reminders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Reminders");
        }
    }
}
