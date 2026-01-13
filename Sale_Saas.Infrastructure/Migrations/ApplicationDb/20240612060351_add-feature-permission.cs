using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addfeaturepermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeatureId",
                table: "ApplicationRoleDetails",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

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
                name: "IX_ApplicationRoleDetails_FeatureId",
                table: "ApplicationRoleDetails",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureMenu_MenusId",
                table: "FeatureMenu",
                column: "MenusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoleDetails_Features_FeatureId",
                table: "ApplicationRoleDetails",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRoleDetails_Features_FeatureId",
                table: "ApplicationRoleDetails");

            migrationBuilder.DropTable(
                name: "FeatureMenu");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationRoleDetails_FeatureId",
                table: "ApplicationRoleDetails");

            migrationBuilder.DropColumn(
                name: "FeatureId",
                table: "ApplicationRoleDetails");
        }
    }
}
