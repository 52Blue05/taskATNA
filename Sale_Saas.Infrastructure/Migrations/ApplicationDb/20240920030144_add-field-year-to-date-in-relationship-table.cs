using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addfieldyeartodateinrelationshiptable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "YearToDateId",
                table: "Relationships",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_YearToDateId",
                table: "Relationships",
                column: "YearToDateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_RelationshipLevels_YearToDateId",
                table: "Relationships",
                column: "YearToDateId",
                principalTable: "RelationshipLevels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_RelationshipLevels_YearToDateId",
                table: "Relationships");

            migrationBuilder.DropIndex(
                name: "IX_Relationships_YearToDateId",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "YearToDateId",
                table: "Relationships");
        }
    }
}
