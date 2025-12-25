using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addrelationalbetweenroleandbenefit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RolePosition",
                table: "Benefits");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationRoleId",
                table: "Benefits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "Benefits",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Benefits_ApplicationRoleId",
                table: "Benefits",
                column: "ApplicationRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Benefits_ApplicationRoles_ApplicationRoleId",
                table: "Benefits",
                column: "ApplicationRoleId",
                principalTable: "ApplicationRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Benefits_ApplicationRoles_ApplicationRoleId",
                table: "Benefits");

            migrationBuilder.DropIndex(
                name: "IX_Benefits_ApplicationRoleId",
                table: "Benefits");

            migrationBuilder.DropColumn(
                name: "ApplicationRoleId",
                table: "Benefits");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Benefits");

            migrationBuilder.AddColumn<string>(
                name: "RolePosition",
                table: "Benefits",
                type: "text",
                nullable: true);
        }
    }
}
