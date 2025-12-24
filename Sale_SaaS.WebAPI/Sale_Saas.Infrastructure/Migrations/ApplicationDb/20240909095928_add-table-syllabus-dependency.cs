using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addtablesyllabusdependency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyllabusDependencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrerequisiteSyllabusId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyllabusDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyllabusDependencies_Syllabus_PrerequisiteSyllabusId",
                        column: x => x.PrerequisiteSyllabusId,
                        principalTable: "Syllabus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SyllabusDependencies_Syllabus_SyllabusId",
                        column: x => x.SyllabusId,
                        principalTable: "Syllabus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusDependencies_PrerequisiteSyllabusId",
                table: "SyllabusDependencies",
                column: "PrerequisiteSyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusDependencies_SyllabusId",
                table: "SyllabusDependencies",
                column: "SyllabusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyllabusDependencies");
        }
    }
}
