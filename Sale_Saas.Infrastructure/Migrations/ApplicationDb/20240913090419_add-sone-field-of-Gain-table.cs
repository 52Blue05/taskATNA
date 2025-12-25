using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addsonefieldofGaintable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkPlace",
                table: "Relationships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "Gains",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GainsFamily",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Relationship = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    YearOfBirth = table.Column<int>(type: "integer", nullable: true),
                    GainsId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GainsFamily", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GainsFamily_Gains_GainsId",
                        column: x => x.GainsId,
                        principalTable: "Gains",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GainsSchools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    GainsId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GainsSchools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GainsSchools_Gains_GainsId",
                        column: x => x.GainsId,
                        principalTable: "Gains",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GainsFamily_GainsId",
                table: "GainsFamily",
                column: "GainsId");

            migrationBuilder.CreateIndex(
                name: "IX_GainsSchools_GainsId",
                table: "GainsSchools",
                column: "GainsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GainsFamily");

            migrationBuilder.DropTable(
                name: "GainsSchools");

            migrationBuilder.DropColumn(
                name: "WorkPlace",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "Gains");
        }
    }
}
