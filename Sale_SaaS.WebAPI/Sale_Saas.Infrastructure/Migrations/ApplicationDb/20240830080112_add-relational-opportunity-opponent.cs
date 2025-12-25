using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addrelationalopportunityopponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityOpponents_Opponents_OpponentId",
                table: "OpportunityOpponents");

            migrationBuilder.DropTable(
                name: "Opponents");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityOpponents_OpponentId",
                table: "OpportunityOpponents");

            migrationBuilder.DropColumn(
                name: "OpponentId",
                table: "OpportunityOpponents");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "OpportunityOpponents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Strength",
                table: "OpportunityOpponents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Weakness",
                table: "OpportunityOpponents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "OpportunityOpponents");

            migrationBuilder.DropColumn(
                name: "Strength",
                table: "OpportunityOpponents");

            migrationBuilder.DropColumn(
                name: "Weakness",
                table: "OpportunityOpponents");

            migrationBuilder.AddColumn<Guid>(
                name: "OpponentId",
                table: "OpportunityOpponents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Opponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Strength = table.Column<string>(type: "text", nullable: true),
                    Weakness = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opponents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityOpponents_OpponentId",
                table: "OpportunityOpponents",
                column: "OpponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityOpponents_Opponents_OpponentId",
                table: "OpportunityOpponents",
                column: "OpponentId",
                principalTable: "Opponents",
                principalColumn: "Id");
        }
    }
}
