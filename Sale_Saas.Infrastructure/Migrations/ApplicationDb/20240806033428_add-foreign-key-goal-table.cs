using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addforeignkeygoaltable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Goals_ApplicationRoleId",
                table: "Goals",
                column: "ApplicationRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_ApplicationRoles_ApplicationRoleId",
                table: "Goals",
                column: "ApplicationRoleId",
                principalTable: "ApplicationRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_ApplicationRoles_ApplicationRoleId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_ApplicationRoleId",
                table: "Goals");
        }
    }
}
