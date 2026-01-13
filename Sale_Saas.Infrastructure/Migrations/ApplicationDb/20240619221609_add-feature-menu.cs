using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addfeaturemenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureMenu");

            migrationBuilder.CreateTable(
                name: "FeatureMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureId = table.Column<string>(type: "text", nullable: true),
                    MenuId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureMenus_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeatureMenus_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureMenus_FeatureId",
                table: "FeatureMenus",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureMenus_MenuId",
                table: "FeatureMenus",
                column: "MenuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureMenus");

            migrationBuilder.CreateTable(
                name: "FeatureMenu",
                columns: table => new
                {
                    FeaturesId = table.Column<string>(type: "text", nullable: false),
                    MenusId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureMenu", x => new { x.FeaturesId, x.MenusId });
                    table.ForeignKey(
                        name: "FK_FeatureMenu_Features_FeaturesId",
                        column: x => x.FeaturesId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeatureMenu_Menus_MenusId",
                        column: x => x.MenusId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureMenu_MenusId",
                table: "FeatureMenu",
                column: "MenusId");
        }
    }
}
