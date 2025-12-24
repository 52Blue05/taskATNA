using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addroleofgoaltable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationRoleId",
                table: "Goals",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationRoleId",
                table: "Goals");
        }
    }
}
