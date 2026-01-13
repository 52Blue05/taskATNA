using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addfkemployeesalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalarys_RoleId",
                table: "EmployeeSalarys",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalarys_UserId",
                table: "EmployeeSalarys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryDetails_UserId",
                table: "EmployeeSalaryDetails",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSalaryDetails_ApplicationUsers_UserId",
                table: "EmployeeSalaryDetails",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSalarys_ApplicationRoles_RoleId",
                table: "EmployeeSalarys",
                column: "RoleId",
                principalTable: "ApplicationRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSalarys_ApplicationUsers_UserId",
                table: "EmployeeSalarys",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSalaryDetails_ApplicationUsers_UserId",
                table: "EmployeeSalaryDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSalarys_ApplicationRoles_RoleId",
                table: "EmployeeSalarys");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSalarys_ApplicationUsers_UserId",
                table: "EmployeeSalarys");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalarys_RoleId",
                table: "EmployeeSalarys");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalarys_UserId",
                table: "EmployeeSalarys");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalaryDetails_UserId",
                table: "EmployeeSalaryDetails");
        }
    }
}
