using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addlinkfieldoflessontable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFile",
                table: "Lessions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Lessions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFile",
                table: "Lessions");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "Lessions");
        }
    }
}
