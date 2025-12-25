using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addrelationshiphistorytable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RelationshipHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_RelationshipHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelationshipHistories_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RelationshipHistories_RelationshipStatuses_PreviousStatusId",
                        column: x => x.PreviousStatusId,
                        principalTable: "RelationshipStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RelationshipHistories_RelationshipStatuses_UpdatedStatusId",
                        column: x => x.UpdatedStatusId,
                        principalTable: "RelationshipStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RelationshipHistories_Relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationships",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipHistories_ApplicationUserId",
                table: "RelationshipHistories",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipHistories_PreviousStatusId",
                table: "RelationshipHistories",
                column: "PreviousStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipHistories_RelationshipId",
                table: "RelationshipHistories",
                column: "RelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipHistories_UpdatedStatusId",
                table: "RelationshipHistories",
                column: "UpdatedStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelationshipHistories");
        }
    }
}
