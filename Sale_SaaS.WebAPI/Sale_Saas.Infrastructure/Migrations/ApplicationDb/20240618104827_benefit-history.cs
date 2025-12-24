using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class benefithistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BenefitHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BenefitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousStatusId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedStatusId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenefitHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BenefitHistories_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BenefitHistories_BenefitStatuses_PreviousStatusId",
                        column: x => x.PreviousStatusId,
                        principalTable: "BenefitStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BenefitHistories_BenefitStatuses_UpdatedStatusId",
                        column: x => x.UpdatedStatusId,
                        principalTable: "BenefitStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BenefitHistories_Benefits_BenefitId",
                        column: x => x.BenefitId,
                        principalTable: "Benefits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenefitHistories_ApplicationUserId",
                table: "BenefitHistories",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BenefitHistories_BenefitId",
                table: "BenefitHistories",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_BenefitHistories_PreviousStatusId",
                table: "BenefitHistories",
                column: "PreviousStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_BenefitHistories_UpdatedStatusId",
                table: "BenefitHistories",
                column: "UpdatedStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenefitHistories");
        }
    }
}
