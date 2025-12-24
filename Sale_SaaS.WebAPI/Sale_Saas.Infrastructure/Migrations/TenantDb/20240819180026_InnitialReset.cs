using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale_Saas.Infrastructure.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class InnitialReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleTenants",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Link = table.Column<string>(type: "text", nullable: true),
                    NameController = table.Column<string>(type: "text", nullable: true),
                    NameAction = table.Column<string>(type: "text", nullable: true),
                    Parameter = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<string>(type: "text", nullable: true),
                    BreadcrumbNavigation = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleTenants", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "OtpSends",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNo = table.Column<string>(type: "text", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: true),
                    Otp = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    DateInput = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ResultContent = table.Column<string>(type: "text", nullable: true),
                    ConfirmFlag = table.Column<bool>(type: "boolean", nullable: false),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpSends", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Time = table.Column<int>(type: "integer", nullable: false),
                    LimitChild = table.Column<int>(type: "integer", nullable: true),
                    LimitUser = table.Column<int>(type: "integer", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackingLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FunctionTime = table.Column<string>(type: "text", nullable: true),
                    Method = table.Column<string>(type: "text", nullable: true),
                    ResponseTimeSec = table.Column<float>(type: "real", nullable: true),
                    ResponseTimeMin = table.Column<float>(type: "real", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAdmins",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "character varying(125)", maxLength: 125, nullable: false),
                    Password = table.Column<string>(type: "character varying(125)", maxLength: 125, nullable: false),
                    FullName = table.Column<string>(type: "character varying(125)", maxLength: 125, nullable: false),
                    Email = table.Column<string>(type: "character varying(125)", maxLength: 125, nullable: false),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAdmins", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "UserStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Host = table.Column<string>(type: "text", nullable: true),
                    SubDomain = table.Column<string>(type: "text", nullable: true),
                    Logo = table.Column<string>(type: "text", nullable: true),
                    ThemeColor = table.Column<string>(type: "text", nullable: true),
                    ConnectionString = table.Column<string>(type: "text", nullable: true),
                    IsAdminCreated = table.Column<bool>(type: "boolean", nullable: true),
                    GroupTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenants_GroupTenants_GroupTenantId",
                        column: x => x.GroupTenantId,
                        principalTable: "GroupTenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlanServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Time = table.Column<int>(type: "integer", nullable: false),
                    IsActived = table.Column<bool>(type: "boolean", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_GroupTenants_GroupTenantId",
                        column: x => x.GroupTenantId,
                        principalTable: "GroupTenants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_PlanServices_PlanServiceId",
                        column: x => x.PlanServiceId,
                        principalTable: "PlanServices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanServiceModuleTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModuleTenantId = table.Column<string>(type: "character varying(250)", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanServiceModuleTenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanServiceModuleTenants_ModuleTenants_ModuleTenantId",
                        column: x => x.ModuleTenantId,
                        principalTable: "ModuleTenants",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_PlanServiceModuleTenants_PlanServices_PlanServiceId",
                        column: x => x.PlanServiceId,
                        principalTable: "PlanServices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Password = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LastName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FullName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Phone = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ApplicationUserStatusId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: true),
                    GroupTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceID = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_GroupTenants_GroupTenantId",
                        column: x => x.GroupTenantId,
                        principalTable: "GroupTenants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_UserStatus_ApplicationUserStatusId",
                        column: x => x.ApplicationUserStatusId,
                        principalTable: "UserStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTenants_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModuleTenantId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    NameEn = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    SeenFlag = table.Column<bool>(type: "boolean", nullable: true),
                    ReadFlag = table.Column<bool>(type: "boolean", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    ActionId = table.Column<string>(type: "text", nullable: true),
                    SeenDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Navigate = table.Column<string>(type: "text", nullable: true),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(250)", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            // ✅ FIX: add missing tables for IX_UnitQuestions_UnitId
            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Question = table.Column<string>(type: "text", nullable: true),
                    Answer = table.Column<string>(type: "text", nullable: true),
                    DeleteFlag = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitQuestions_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            // Indexes (giữ nguyên logic bạn đang có)
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TenantId",
                table: "Notifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GroupTenantId",
                table: "Orders",
                column: "GroupTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PlanServiceId",
                table: "Orders",
                column: "PlanServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanServiceModuleTenants_ModuleTenantId",
                table: "PlanServiceModuleTenants",
                column: "ModuleTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanServiceModuleTenants_PlanServiceId",
                table: "PlanServiceModuleTenants",
                column: "PlanServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_GroupTenantId",
                table: "Tenants",
                column: "GroupTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitQuestions_UnitId",
                table: "UnitQuestions",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApplicationUserStatusId",
                table: "Users",
                column: "ApplicationUserStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupTenantId",
                table: "Users",
                column: "GroupTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_TenantId",
                table: "UserTenants",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "OtpSends");

            migrationBuilder.DropTable(
                name: "PlanServiceModuleTenants");

            migrationBuilder.DropTable(
                name: "TrackingLogs");

            migrationBuilder.DropTable(
                name: "UserAdmins");

            migrationBuilder.DropTable(
                name: "UserTenants");

            // ✅ FIX: drop UnitQuestions + Units (added)
            migrationBuilder.DropTable(
                name: "UnitQuestions");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ModuleTenants");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "UserStatus");

            migrationBuilder.DropTable(
                name: "PlanServices");

            migrationBuilder.DropTable(
                name: "GroupTenants");
        }
    }
}
