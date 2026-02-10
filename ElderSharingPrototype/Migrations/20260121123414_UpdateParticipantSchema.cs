using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElderSharingPrototype.Migrations
{
    /// <inheritdoc />
    public partial class UpdateParticipantSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Participants");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "Participants",
                newName: "Phone2");

            migrationBuilder.RenameColumn(
                name: "IdNumber",
                table: "Participants",
                newName: "Phone1");

            migrationBuilder.RenameColumn(
                name: "FixedDiseases",
                table: "Participants",
                newName: "PersonalIdNumber");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Participants",
                newName: "Hmo");

            migrationBuilder.AddColumn<bool>(
                name: "CameraConsent",
                table: "Participants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MicConsent",
                table: "Participants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CameraConsent",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "MicConsent",
                table: "Participants");

            migrationBuilder.RenameColumn(
                name: "Phone2",
                table: "Participants",
                newName: "Language");

            migrationBuilder.RenameColumn(
                name: "Phone1",
                table: "Participants",
                newName: "IdNumber");

            migrationBuilder.RenameColumn(
                name: "PersonalIdNumber",
                table: "Participants",
                newName: "FixedDiseases");

            migrationBuilder.RenameColumn(
                name: "Hmo",
                table: "Participants",
                newName: "FirstName");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Participants",
                type: "int",
                nullable: true);
        }
    }
}
