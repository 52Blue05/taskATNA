using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addfieldtimecompletionofresulttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeCompletion",
                table: "Results",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeCompletion",
                table: "Results");
        }
    }
}
