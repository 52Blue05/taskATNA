using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class updateuserfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ApplicationUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrentPosition",
                table: "ApplicationUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "ApplicationUsers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "ApplicationUsers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "ApplicationUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Review",
                table: "ApplicationUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "ApplicationUsers",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "CurrentPosition",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "Review",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ApplicationUsers");
        }
    }
}
