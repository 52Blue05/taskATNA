using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addadditionalfieldofopportunityandemployeesalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalMoney",
                table: "Opportunities",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeMoney",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountInsurance",
                table: "EmployeeSalarys",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MyDependent",
                table: "EmployeeSalarys",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberDependent",
                table: "EmployeeSalarys",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentInsurance",
                table: "EmployeeSalarys",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitDependent",
                table: "EmployeeSalarys",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalMoney",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "TypeMoney",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "AmountInsurance",
                table: "EmployeeSalarys");

            migrationBuilder.DropColumn(
                name: "MyDependent",
                table: "EmployeeSalarys");

            migrationBuilder.DropColumn(
                name: "NumberDependent",
                table: "EmployeeSalarys");

            migrationBuilder.DropColumn(
                name: "PercentInsurance",
                table: "EmployeeSalarys");

            migrationBuilder.DropColumn(
                name: "UnitDependent",
                table: "EmployeeSalarys");
        }
    }
}
