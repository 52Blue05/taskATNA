using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class positionfeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RolePositionFeatureMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RolePositionId = table.Column<string>(type: "text", nullable: true),
                    FeatureMenuId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePositionFeatureMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePositionFeatureMenus_FeatureMenus_FeatureMenuId",
                        column: x => x.FeatureMenuId,
                        principalTable: "FeatureMenus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolePositionFeatureMenus_RolePositions_RolePositionId",
                        column: x => x.RolePositionId,
                        principalTable: "RolePositions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePositionFeatureMenus_FeatureMenuId",
                table: "RolePositionFeatureMenus",
                column: "FeatureMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePositionFeatureMenus_RolePositionId",
                table: "RolePositionFeatureMenus",
                column: "RolePositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePositionFeatureMenus");
        }
    }
}
