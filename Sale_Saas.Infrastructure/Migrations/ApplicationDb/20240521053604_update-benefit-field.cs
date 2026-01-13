using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class updatebenefitfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SuggestActualSalary",
                table: "Benefits",
                newName: "TotalSalary");

            migrationBuilder.RenameColumn(
                name: "ActualSalary",
                table: "Benefits",
                newName: "SuggestTotalSalary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalSalary",
                table: "Benefits",
                newName: "SuggestActualSalary");

            migrationBuilder.RenameColumn(
                name: "SuggestTotalSalary",
                table: "Benefits",
                newName: "ActualSalary");
        }
    }
}
