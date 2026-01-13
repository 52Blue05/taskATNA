using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addtablecriteriaandrelatedrelational : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CriteriaId",
                table: "Goals",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Criterias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criterias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goals_CriteriaId",
                table: "Goals",
                column: "CriteriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Criterias_CriteriaId",
                table: "Goals",
                column: "CriteriaId",
                principalTable: "Criterias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Criterias_CriteriaId",
                table: "Goals");

            migrationBuilder.DropTable(
                name: "Criterias");

            migrationBuilder.DropIndex(
                name: "IX_Goals_CriteriaId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "CriteriaId",
                table: "Goals");
        }
    }
}
