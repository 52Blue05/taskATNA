using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddFieldRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Relationships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnswerDetail",
                table: "Relationship_GainsQuestions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "AnswerDetail",
                table: "Relationship_GainsQuestions");
        }
    }
}
