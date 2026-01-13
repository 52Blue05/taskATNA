using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addsomefieldtableopportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Opponent1Strength",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent1Weakness",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent2Strength",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent2Weakness",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent3",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent3Attribute",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent3Strength",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent3Weakness",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpportunityEndDate",
                table: "Opportunities",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpportunityStartDate",
                table: "Opportunities",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Opponent1Strength",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent1Weakness",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent2Strength",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent2Weakness",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent3",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent3Attribute",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent3Strength",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent3Weakness",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OpportunityEndDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "OpportunityStartDate",
                table: "Opportunities");
        }
    }
}
