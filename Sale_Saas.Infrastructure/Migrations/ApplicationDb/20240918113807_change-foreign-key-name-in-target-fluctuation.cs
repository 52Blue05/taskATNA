using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class changeforeignkeynameintargetfluctuation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TargetFluctuations_Goals_GoaldId",
                table: "TargetFluctuations");

            migrationBuilder.RenameColumn(
                name: "GoaldId",
                table: "TargetFluctuations",
                newName: "GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_TargetFluctuations_GoaldId",
                table: "TargetFluctuations",
                newName: "IX_TargetFluctuations_GoalId");

            migrationBuilder.AddForeignKey(
                name: "FK_TargetFluctuations_Goals_GoalId",
                table: "TargetFluctuations",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TargetFluctuations_Goals_GoalId",
                table: "TargetFluctuations");

            migrationBuilder.RenameColumn(
                name: "GoalId",
                table: "TargetFluctuations",
                newName: "GoaldId");

            migrationBuilder.RenameIndex(
                name: "IX_TargetFluctuations_GoalId",
                table: "TargetFluctuations",
                newName: "IX_TargetFluctuations_GoaldId");

            migrationBuilder.AddForeignKey(
                name: "FK_TargetFluctuations_Goals_GoaldId",
                table: "TargetFluctuations",
                column: "GoaldId",
                principalTable: "Goals",
                principalColumn: "Id");
        }
    }
}
