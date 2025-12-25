using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addfieldsyllabusidofunitdependencytable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SyllabusId",
                table: "UnitDependencies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitDependencies_SyllabusId",
                table: "UnitDependencies",
                column: "SyllabusId");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitDependencies_Syllabus_SyllabusId",
                table: "UnitDependencies",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitDependencies_Syllabus_SyllabusId",
                table: "UnitDependencies");

            migrationBuilder.DropIndex(
                name: "IX_UnitDependencies_SyllabusId",
                table: "UnitDependencies");

            migrationBuilder.DropColumn(
                name: "SyllabusId",
                table: "UnitDependencies");
        }
    }
}
