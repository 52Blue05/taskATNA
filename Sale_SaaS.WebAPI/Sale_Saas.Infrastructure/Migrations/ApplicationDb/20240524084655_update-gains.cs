using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class updategains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Gains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeekendActivity",
                table: "Gains",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Gains");

            migrationBuilder.DropColumn(
                name: "WeekendActivity",
                table: "Gains");
        }
    }
}
