using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class updateproject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Services_ServiceId",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "Projects",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "ResponsiblePerson",
                table: "Projects",
                newName: "Service");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_ServiceId",
                table: "Projects",
                newName: "IX_Projects_ApplicationUserId");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "ApplicationUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ApplicationUsers_ApplicationUserId",
                table: "Projects",
                column: "ApplicationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ApplicationUsers_ApplicationUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "ApplicationUsers");

            migrationBuilder.RenameColumn(
                name: "Service",
                table: "Projects",
                newName: "ResponsiblePerson");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Projects",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_ApplicationUserId",
                table: "Projects",
                newName: "IX_Projects_ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Services_ServiceId",
                table: "Projects",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }
    }
}
