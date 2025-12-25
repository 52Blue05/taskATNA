using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class removesomefieldintargetfluctuation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionPercent",
                table: "TargetFluctuations");

            migrationBuilder.DropColumn(
                name: "TargetSalary",
                table: "TargetFluctuations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletionPercent",
                table: "TargetFluctuations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetSalary",
                table: "TargetFluctuations",
                type: "text",
                nullable: true);
        }
    }
}
