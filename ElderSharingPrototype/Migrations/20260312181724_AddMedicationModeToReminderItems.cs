using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElderSharingPrototype.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationModeToReminderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedicationMode",
                table: "ReminderItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicationMode",
                table: "ReminderItems");
        }
    }
}
