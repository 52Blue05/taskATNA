using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class adduserrelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "Relationships",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ApplicationUserId",
                table: "Relationships",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_ApplicationUsers_ApplicationUserId",
                table: "Relationships",
                column: "ApplicationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_ApplicationUsers_ApplicationUserId",
                table: "Relationships");

            migrationBuilder.DropIndex(
                name: "IX_Relationships_ApplicationUserId",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Relationships");
        }
    }
}
