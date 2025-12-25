using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class adduseropportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "OpportunityHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityHistories_ApplicationUserId",
                table: "OpportunityHistories",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityHistories_ApplicationUsers_ApplicationUserId",
                table: "OpportunityHistories",
                column: "ApplicationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityHistories_ApplicationUsers_ApplicationUserId",
                table: "OpportunityHistories");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityHistories_ApplicationUserId",
                table: "OpportunityHistories");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "OpportunityHistories");
        }
    }
}
