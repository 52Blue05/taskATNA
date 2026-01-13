using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addroleposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RolePositionId",
                table: "ApplicationRoles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RolePositions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_RolePositionId",
                table: "ApplicationRoles",
                column: "RolePositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoles_RolePositions_RolePositionId",
                table: "ApplicationRoles",
                column: "RolePositionId",
                principalTable: "RolePositions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRoles_RolePositions_RolePositionId",
                table: "ApplicationRoles");

            migrationBuilder.DropTable(
                name: "RolePositions");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationRoles_RolePositionId",
                table: "ApplicationRoles");

            migrationBuilder.DropColumn(
                name: "RolePositionId",
                table: "ApplicationRoles");
        }
    }
}
