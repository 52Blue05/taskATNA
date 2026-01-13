using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class updatefieldsuggest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Relationships");

            migrationBuilder.AddColumn<DateTime>(
                name: "SuggestEndTime",
                table: "Goals",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SuggestActualSalary",
                table: "Benefits",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SuggestMonthlySalary",
                table: "Benefits",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SuggestTargetSalary",
                table: "Benefits",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuggestEndTime",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "SuggestActualSalary",
                table: "Benefits");

            migrationBuilder.DropColumn(
                name: "SuggestMonthlySalary",
                table: "Benefits");

            migrationBuilder.DropColumn(
                name: "SuggestTargetSalary",
                table: "Benefits");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Relationships",
                type: "integer",
                nullable: true);
        }
    }
}
