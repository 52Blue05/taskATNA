using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class changefieldinrelationshiphistorytable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelationshipHistories_RelationshipStatuses_PreviousStatusId",
                table: "RelationshipHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_RelationshipHistories_RelationshipStatuses_UpdatedStatusId",
                table: "RelationshipHistories");

            migrationBuilder.RenameColumn(
                name: "UpdatedStatusId",
                table: "RelationshipHistories",
                newName: "UpdatedLevelId");

            migrationBuilder.RenameColumn(
                name: "PreviousStatusId",
                table: "RelationshipHistories",
                newName: "PreviousLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_RelationshipHistories_UpdatedStatusId",
                table: "RelationshipHistories",
                newName: "IX_RelationshipHistories_UpdatedLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_RelationshipHistories_PreviousStatusId",
                table: "RelationshipHistories",
                newName: "IX_RelationshipHistories_PreviousLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelationshipHistories_RelationshipLevels_PreviousLevelId",
                table: "RelationshipHistories",
                column: "PreviousLevelId",
                principalTable: "RelationshipLevels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RelationshipHistories_RelationshipLevels_UpdatedLevelId",
                table: "RelationshipHistories",
                column: "UpdatedLevelId",
                principalTable: "RelationshipLevels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelationshipHistories_RelationshipLevels_PreviousLevelId",
                table: "RelationshipHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_RelationshipHistories_RelationshipLevels_UpdatedLevelId",
                table: "RelationshipHistories");

            migrationBuilder.RenameColumn(
                name: "UpdatedLevelId",
                table: "RelationshipHistories",
                newName: "UpdatedStatusId");

            migrationBuilder.RenameColumn(
                name: "PreviousLevelId",
                table: "RelationshipHistories",
                newName: "PreviousStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_RelationshipHistories_UpdatedLevelId",
                table: "RelationshipHistories",
                newName: "IX_RelationshipHistories_UpdatedStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_RelationshipHistories_PreviousLevelId",
                table: "RelationshipHistories",
                newName: "IX_RelationshipHistories_PreviousStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelationshipHistories_RelationshipStatuses_PreviousStatusId",
                table: "RelationshipHistories",
                column: "PreviousStatusId",
                principalTable: "RelationshipStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RelationshipHistories_RelationshipStatuses_UpdatedStatusId",
                table: "RelationshipHistories",
                column: "UpdatedStatusId",
                principalTable: "RelationshipStatuses",
                principalColumn: "Id");
        }
    }
}
