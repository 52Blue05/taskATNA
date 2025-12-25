using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addrelationalcustomerandopportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_CustomerId",
                table: "Opportunities",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Customers_CustomerId",
                table: "Opportunities",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Customers_CustomerId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_CustomerId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Opportunities");
        }
    }
}
