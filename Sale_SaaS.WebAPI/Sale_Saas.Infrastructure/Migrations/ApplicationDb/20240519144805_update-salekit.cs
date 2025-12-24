using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class updatesalekit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Path",
                table: "SaleKits",
                newName: "OriginalFileName");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "SaleKits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SaleKits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "SaleKits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "SaleKits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "SaleKits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FolderName",
                table: "SaleKits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IncomeOther",
                table: "EmployeeSalarys",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Access",
                table: "ApplicationRole_SaleKits",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeSalaryDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeCode = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Month = table.Column<int>(type: "integer", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    IncomeEta = table.Column<decimal>(type: "numeric", nullable: true),
                    IncomeReal = table.Column<decimal>(type: "numeric", nullable: true),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TypeCP = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ProjectName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TimeSpent = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalaryDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSalaryDetails");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "SaleKits");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SaleKits");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "SaleKits");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "SaleKits");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "SaleKits");

            migrationBuilder.DropColumn(
                name: "FolderName",
                table: "SaleKits");

            migrationBuilder.DropColumn(
                name: "IncomeOther",
                table: "EmployeeSalarys");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "ApplicationRole_SaleKits");

            migrationBuilder.RenameColumn(
                name: "OriginalFileName",
                table: "SaleKits",
                newName: "Path");
        }
    }
}
