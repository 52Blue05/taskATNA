using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addtablerelationshipcustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelationshipCustomerId",
                table: "Relationships",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RelationshipCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationshipCustomers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_RelationshipCustomerId",
                table: "Relationships",
                column: "RelationshipCustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_RelationshipCustomers_RelationshipCustomerId",
                table: "Relationships",
                column: "RelationshipCustomerId",
                principalTable: "RelationshipCustomers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_RelationshipCustomers_RelationshipCustomerId",
                table: "Relationships");

            migrationBuilder.DropTable(
                name: "RelationshipCustomers");

            migrationBuilder.DropIndex(
                name: "IX_Relationships_RelationshipCustomerId",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "RelationshipCustomerId",
                table: "Relationships");
        }
    }
}
