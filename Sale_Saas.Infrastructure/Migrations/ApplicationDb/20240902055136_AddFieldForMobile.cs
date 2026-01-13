using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddFieldForMobile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GoalId",
                table: "Relationships",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BenefitId",
                table: "Goals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CriteriaType",
                table: "Goals",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoalId",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "BenefitId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "CriteriaType",
                table: "Goals");
        }
    }
}
