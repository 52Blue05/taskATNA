using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class Updaterole_relationship_opportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationRoleId",
                table: "Relationships",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationRoleId",
                table: "Opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ApplicationRoleId",
                table: "Relationships",
                column: "ApplicationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_ApplicationRoleId",
                table: "Opportunities",
                column: "ApplicationRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_ApplicationRoles_ApplicationRoleId",
                table: "Opportunities",
                column: "ApplicationRoleId",
                principalTable: "ApplicationRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_ApplicationRoles_ApplicationRoleId",
                table: "Relationships",
                column: "ApplicationRoleId",
                principalTable: "ApplicationRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_ApplicationRoles_ApplicationRoleId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_ApplicationRoles_ApplicationRoleId",
                table: "Relationships");

            migrationBuilder.DropIndex(
                name: "IX_Relationships_ApplicationRoleId",
                table: "Relationships");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_ApplicationRoleId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ApplicationRoleId",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "ApplicationRoleId",
                table: "Opportunities");
        }
    }
}
