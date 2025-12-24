using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class updatefieldopportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Customers_AccountableId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_Customers_CustomerId",
                table: "Relationships");

            migrationBuilder.DropIndex(
                name: "IX_Relationships_CustomerId",
                table: "Relationships");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_AccountableId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "AccountableId",
                table: "Opportunities");

            migrationBuilder.RenameColumn(
                name: "Opponents",
                table: "Opportunities",
                newName: "Opponent2Attribute");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Relationships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Accountable",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Beneficiary",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent1",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent1Attribute",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opponent2",
                table: "Opportunities",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Relationships");

            migrationBuilder.DropColumn(
                name: "Accountable",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Beneficiary",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent1",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent1Attribute",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Opponent2",
                table: "Opportunities");

            migrationBuilder.RenameColumn(
                name: "Opponent2Attribute",
                table: "Opportunities",
                newName: "Opponents");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Relationships",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountableId",
                table: "Opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_CustomerId",
                table: "Relationships",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_AccountableId",
                table: "Opportunities",
                column: "AccountableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Customers_AccountableId",
                table: "Opportunities",
                column: "AccountableId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_Customers_CustomerId",
                table: "Relationships",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }
    }
}
