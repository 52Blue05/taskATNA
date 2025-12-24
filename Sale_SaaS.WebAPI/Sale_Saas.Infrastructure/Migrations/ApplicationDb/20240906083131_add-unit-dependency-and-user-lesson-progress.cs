using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class addunitdependencyanduserlessonprogress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnitDependencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrerequisiteUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitDependencies_Units_PrerequisiteUnitId",
                        column: x => x.PrerequisiteUnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnitDependencies_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserLessonProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDone = table.Column<bool>(type: "boolean", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLessonProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLessonProgresses_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserLessonProgresses_Lessions_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitDependencies_PrerequisiteUnitId",
                table: "UnitDependencies",
                column: "PrerequisiteUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitDependencies_UnitId",
                table: "UnitDependencies",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonProgresses_ApplicationUserId",
                table: "UserLessonProgresses",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonProgresses_LessonId",
                table: "UserLessonProgresses",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitDependencies");

            migrationBuilder.DropTable(
                name: "UserLessonProgresses");
        }
    }
}
