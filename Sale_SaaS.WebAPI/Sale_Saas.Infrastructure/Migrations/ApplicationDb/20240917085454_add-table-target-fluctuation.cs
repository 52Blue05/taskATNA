using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addtabletargetfluctuation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimateBenefit",
                table: "Benefits",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TargetFluctuations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetYear = table.Column<int>(type: "integer", nullable: true),
                    TypeMoney = table.Column<string>(type: "text", nullable: true),
                    TargetSalary = table.Column<string>(type: "text", nullable: true),
                    CompletionPercent = table.Column<string>(type: "text", nullable: true),
                    BenefitId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoaldId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetFluctuations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetFluctuations_Benefits_BenefitId",
                        column: x => x.BenefitId,
                        principalTable: "Benefits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TargetFluctuations_Goals_GoaldId",
                        column: x => x.GoaldId,
                        principalTable: "Goals",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TargetFluctuations_BenefitId",
                table: "TargetFluctuations",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetFluctuations_GoaldId",
                table: "TargetFluctuations",
                column: "GoaldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetFluctuations");

            migrationBuilder.DropColumn(
                name: "EstimateBenefit",
                table: "Benefits");
        }
    }
}
