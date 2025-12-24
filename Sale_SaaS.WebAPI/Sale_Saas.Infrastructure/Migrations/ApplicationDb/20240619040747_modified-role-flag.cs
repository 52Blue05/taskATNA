using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class modifiedroleflag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsModified",
                table: "RolePositions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RolePosition",
                table: "Relationships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RolePosition",
                table: "Goals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RolePosition",
                table: "Benefits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsModified",
                table: "ApplicationRoles",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsModified",
                table: "RolePositions");

            migrationBuilder.DropColumn(
                name: "RolePosition",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "RolePosition",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "RolePosition",
                table: "Benefits");

            migrationBuilder.DropColumn(
                name: "IsModified",
                table: "ApplicationRoles");
        }
    }
}
