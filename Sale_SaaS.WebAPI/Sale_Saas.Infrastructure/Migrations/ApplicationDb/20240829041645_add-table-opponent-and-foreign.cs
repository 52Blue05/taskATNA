using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addtableopponentandforeign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Opponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Strength = table.Column<string>(type: "text", nullable: true),
                    Weakness = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityOpponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpponentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityOpponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityOpponents_Opponents_OpponentId",
                        column: x => x.OpponentId,
                        principalTable: "Opponents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpportunityOpponents_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityOpponents_OpponentId",
                table: "OpportunityOpponents",
                column: "OpponentId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityOpponents_OpportunityId",
                table: "OpportunityOpponents",
                column: "OpportunityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunityOpponents");

            migrationBuilder.DropTable(
                name: "Opponents");
        }
    }
}
