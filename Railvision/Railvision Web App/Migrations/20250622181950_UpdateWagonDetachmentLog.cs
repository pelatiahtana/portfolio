using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainGenie.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWagonDetachmentLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Actions",
                table: "WagonDetachmentLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "WagonDetachmentLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReportedBy",
                table: "WagonDetachmentLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "WagonDetachmentLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "WagonDetachmentLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrainId",
                table: "WagonDetachmentLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actions",
                table: "WagonDetachmentLogs");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "WagonDetachmentLogs");

            migrationBuilder.DropColumn(
                name: "ReportedBy",
                table: "WagonDetachmentLogs");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "WagonDetachmentLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WagonDetachmentLogs");

            migrationBuilder.DropColumn(
                name: "TrainId",
                table: "WagonDetachmentLogs");
        }
    }
}
