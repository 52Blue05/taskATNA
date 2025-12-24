using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;
using System.Data;
namespace Sale_Saas.Infrastructure.Data;

public class ApplicationDbContextInitialiser : IApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationUserStatusConstant _applicationUserStatusConstant;
    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger,
                                            ApplicationDbContext context,
                                            UserManager<ApplicationUser> userManager,
                                            RoleManager<ApplicationRole> roleManager,
                                            IOptions<ApplicationUserStatusConstant> applicationUserStatusConstant
                                            )
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _applicationUserStatusConstant = applicationUserStatusConstant.Value;
    }

    public async Task SeedAsync(string sConnect, List<string> listMenuActive = null)
    {
        try
        {
            _context.SetConnectString(sConnect);
            _context.CurrentTenantConnectionString = sConnect;
            await TrySeedAsync(listMenuActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private List<FeatureMenu> getFeatureMenus(List<Feature> features, Menu menu)
    {
        var lstFeatureMenu = new List<FeatureMenu>();
        foreach (var item in features)
        {
            FeatureMenu featureMenu = new FeatureMenu()
            {
                Id = Guid.NewGuid(),
                FeatureId = item.Id,
                MenuId = menu.Id
            };
            lstFeatureMenu.Add(featureMenu);
        }
        return lstFeatureMenu;
    }

    private async Task TrySeedAsync(List<string> listMenuActive = null)
    {
        #region ApplicationUserStatus
        if (!_context.ApplicationUserStatuses.Any()
            && _applicationUserStatusConstant != null
            && !string.IsNullOrEmpty(_applicationUserStatusConstant.InActiveCode)
            && !string.IsNullOrEmpty(_applicationUserStatusConstant.ActiveCode))
        {
            _context.ApplicationUserStatuses.AddRange(new List<ApplicationUserStatus>() {
                new ApplicationUserStatus() { Id = _applicationUserStatusConstant.InActive, Code = UserStatusEnum.UNACTIVE.ToString(), Name = "Ngừng hoạt động" },
                new ApplicationUserStatus() { Id = _applicationUserStatusConstant.Active, Code = UserStatusEnum.ACTIVE.ToString(), Name = "Đang hoạt động" }
            });
            await _context.SaveChangesAsync();
        }
        #endregion

        #region RolePosition
        if (!_context.RolePositions.Any())
        {
            var admin = new RolePosition()
            {
                Id = RolePositionEnum.ADMIN.ToString(),
                Name = "Người quản trị",
                Level = 0,
                IsModified = false,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                DeleteFlag = false
            };
            _context.RolePositions.Add(admin);

            var adminstrator = new RolePosition()
            {
                Id = RolePositionEnum.ADMINISTRATOR.ToString(),
                Name = "Người cập nhật",
                Level = 1,
                IsModified = true,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                DeleteFlag = false
            };
            _context.RolePositions.Add(adminstrator);

            var manager = new RolePosition()
            {
                Id = RolePositionEnum.MANAGER.ToString(),
                Name = "Giám đốc",
                Level = 2,
                IsModified = true,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                DeleteFlag = false
            };
            _context.RolePositions.Add(manager);

            var employee = new RolePosition()
            {
                Id = RolePositionEnum.EMPLOYEE.ToString(),
                Name = "Nhân viên",
                Level = 3,
                IsModified = true,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                DeleteFlag = false
            };
            _context.RolePositions.Add(employee);

            await _context.SaveChangesAsync();
        }

        #endregion

        #region ApplicationRole
        var administratorRole = new ApplicationRole()
        {
            DisplayName = ApplicationRoleConstant.Administrator,
            Name = ApplicationRoleConstant.Administrator,
            NormalizedName = ApplicationRoleConstant.Administrator,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now,
            Description = ApplicationRoleConstant.Administrator
        };

        var p_admin = await _context.RolePositions.FirstOrDefaultAsync(s => s.Id == RolePositionEnum.ADMIN.ToString());
        var p_adminstrator = await _context.RolePositions.FirstOrDefaultAsync(s => s.Id == RolePositionEnum.ADMINISTRATOR.ToString());
        var p_manager = await _context.RolePositions.FirstOrDefaultAsync(s => s.Id == RolePositionEnum.MANAGER.ToString());
        var p_employee = await _context.RolePositions.FirstOrDefaultAsync(s => s.Id == RolePositionEnum.EMPLOYEE.ToString());


        if (!_roleManager.Roles.Any())
        {
            var saleDirector = new ApplicationRole()
            {
                DisplayName = "Giám đốc bán hàng",
                Name = "SaleDirector",
                NormalizedName = "SALEDIRECTOR",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = "Giám đốc bán hàng",
                IsModified = true,
                RolePositionId = p_manager != null ? p_manager.Id : null
            };
            await _roleManager.CreateAsync(saleDirector);

            var sale = new ApplicationRole()
            {
                DisplayName = "Nhân viên Sale",
                Name = "Sale",
                NormalizedName = "SALE",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = "Nhân viên Sale",
                IsModified = true,
                RolePositionId = p_employee != null ? p_employee.Id : null
            };
            await _roleManager.CreateAsync(sale);

            var supplier = new ApplicationRole()
            {
                DisplayName = "Nhà phân phối",
                Name = "Supplier",
                NormalizedName = "SUPPLIER",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = "Nhà phân phối",
                IsModified = true,
                RolePositionId = p_employee != null ? p_employee.Id : null
            };
            await _roleManager.CreateAsync(supplier);

            var admintrator = new ApplicationRole()
            {
                DisplayName = "Người cập nhật",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = "Người cập nhật",
                IsModified = true,
                RolePositionId = p_adminstrator != null ? p_adminstrator.Id : null
            };
            await _roleManager.CreateAsync(admintrator);

            var admin = new ApplicationRole()
            {
                DisplayName = "Người quản trị",
                Name = "Admin",
                NormalizedName = "ADMIN",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = "Người quản trị",
                IsModified = false,
                RolePositionId = p_admin != null ? p_admin.Id : null
            };
            await _roleManager.CreateAsync(admin);

            await _context.SaveChangesAsync();
        }

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region ApplicationUser
        //// Default users
        //Guid test = Guid.NewGuid();
        //var hasher = new PasswordHasher<ApplicationUser>();
        //var testAccount = new ApplicationUser()
        //{
        //	Id = test,
        //	UserName = "test123@gmail.com",
        //	NormalizedUserName = "test123@gmail.com",
        //	Email = "test123@gmail.com",
        //	NormalizedEmail = "test123@gmail.com",
        //	EmailConfirmed = true,
        //	PasswordHash = hasher.HashPassword(null, "123"),
        //	SecurityStamp = string.Empty,
        //	FirstName = "Test",
        //	LastName = "Tài khoản",
        //	PhoneNumber = "0799723456",
        //	ApplicationUserStatusId = (_applicationUserStatusConstant != null ? _applicationUserStatusConstant.Active : null),
        //	Avatar = string.Empty,
        //	Code = "SA"

        //};

        //if (_userManager.Users.All(u => u.UserName != testAccount.UserName))
        //{
        //	await _userManager.CreateAsync(testAccount, "123");
        //	if (!string.IsNullOrWhiteSpace(administratorRole.Name))
        //	{
        //		await _userManager.AddToRolesAsync(testAccount, new[] { administratorRole.Name });
        //	}
        //	await _context.SaveChangesAsync();
        //}

        #endregion

        #region Feature
        await InitFeature();
        #endregion

        #region Menu
        await InitMenu(listMenuActive);
        #endregion

        #region FeatureMenu
        await InitFeatureMenu();
        #endregion

        #region RolePositionFeatureMenu
        await InitRolePositionFeatureMenu();
        #endregion

        #region ApplicationRoleDetails

        await InitApplicationRoleDetail();

        #endregion

        #region GoalStatus
        if (!_context.GoalStatuses.Any())
        {
            var pending = new GoalStatus
            {
                Id = Guid.NewGuid(),
                Code = GoalStatusEnum.PENDING.ToString(),
                Name = "Đợi chốt mục tiêu",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.GoalStatuses.Add(pending);

            var processing = new GoalStatus
            {
                Id = Guid.NewGuid(),
                Code = GoalStatusEnum.PROCESSING.ToString(),
                Name = "Đang thực hiện",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.GoalStatuses.Add(processing);

            var request = new GoalStatus
            {
                Id = Guid.NewGuid(),
                Code = GoalStatusEnum.REQUEST.ToString(),
                Name = "Yêu cầu chỉnh sửa",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.GoalStatuses.Add(request);

            var updated = new GoalStatus
            {
                Id = Guid.NewGuid(),
                Code = GoalStatusEnum.UPDATED.ToString(),
                Name = "Đã chỉnh sửa",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.GoalStatuses.Add(updated);

            var completed = new GoalStatus
            {
                Id = Guid.NewGuid(),
                Code = GoalStatusEnum.COMPLETED.ToString(),
                Name = "Hoàn thành",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.GoalStatuses.Add(completed);

            var failed = new GoalStatus
            {
                Id = Guid.NewGuid(),
                Code = GoalStatusEnum.FAILED.ToString(),
                Name = "Chưa hoàn thành",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.GoalStatuses.Add(failed);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region BenefitStatus
        if (!_context.BenefitStatuses.Any())
        {
            var pending = new BenefitStatus
            {
                Id = Guid.NewGuid(),
                Code = BenefitStatusEnum.PENDING.ToString(),
                Name = "Đợi chốt quyền lợi",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.BenefitStatuses.Add(pending);

            var updated = new BenefitStatus
            {
                Id = Guid.NewGuid(),
                Code = BenefitStatusEnum.UPDATED.ToString(),
                Name = "Đã chỉnh sửa",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.BenefitStatuses.Add(updated);

            var request = new BenefitStatus
            {
                Id = Guid.NewGuid(),
                Code = BenefitStatusEnum.REQUEST.ToString(),
                Name = "Yêu cầu chỉnh sửa",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.BenefitStatuses.Add(request);

            var confirmed = new BenefitStatus
            {
                Id = Guid.NewGuid(),
                Code = BenefitStatusEnum.CONFIRMED.ToString(),
                Name = "Chốt quyền lợi",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.BenefitStatuses.Add(confirmed);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region OpportunityStatus
        if (!_context.OpportunityStatuses.Any())
        {
            var cancel = new OpportunityStatus
            {
                Id = Guid.NewGuid(),
                Code = OpportunityStatusEnum.CANCEL.ToString(),
                Name = "Cancel",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.OpportunityStatuses.Add(cancel);

            var close = new OpportunityStatus
            {
                Id = Guid.NewGuid(),
                Code = OpportunityStatusEnum.CLOSE.ToString(),
                Name = "Close",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.OpportunityStatuses.Add(close);

            var active = new OpportunityStatus
            {
                Id = Guid.NewGuid(),
                Code = OpportunityStatusEnum.ACTIVE.ToString(),
                Name = "Đang active",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.OpportunityStatuses.Add(active);

            var onhold = new OpportunityStatus
            {
                Id = Guid.NewGuid(),
                Code = OpportunityStatusEnum.ONHOLD.ToString(),
                Name = "On Hold",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.OpportunityStatuses.Add(onhold);

            var fail = new OpportunityStatus
            {
                Id = Guid.NewGuid(),
                Code = OpportunityStatusEnum.FAIL.ToString(),
                Name = "Fail",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.OpportunityStatuses.Add(fail);

            var pending = new OpportunityStatus
            {
                Id = Guid.NewGuid(),
                Code = OpportunityStatusEnum.PENDING.ToString(),
                Name = "Pending",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };
            _context.OpportunityStatuses.Add(pending);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region RelationshipStatus
        if (!_context.RelationshipStatuses.Any())
        {
            var pending = new RelationshipStatus
            {
                Id = Guid.NewGuid(),
                Code = RelationshipStatusEnum.PENDING.ToString(),
                Name = "Đợi chốt",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipStatuses.Add(pending);

            var completed = new RelationshipStatus
            {
                Id = Guid.NewGuid(),
                Code = RelationshipStatusEnum.COMPLETED.ToString(),
                Name = "Hoàn thành",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipStatuses.Add(completed);

            var processing = new RelationshipStatus
            {
                Id = Guid.NewGuid(),
                Code = RelationshipStatusEnum.PROCESSING.ToString(),
                Name = "Đang thực hiện",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipStatuses.Add(processing);

            var confirmed = new RelationshipStatus
            {
                Id = Guid.NewGuid(),
                Code = RelationshipStatusEnum.CONFIRMED.ToString(),
                Name = "Chốt đề xuất",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipStatuses.Add(confirmed);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region RelationshipLevel
        if (!_context.RelationshipLevels.Any())
        {
            var rela_1 = new RelationshipLevel
            {
                Id = Guid.NewGuid(),
                Code = "A",
                Description = "Mới có thông tin liên hệ và chưa có sự chủ động nhờ vã nào",
                Review = "Từ 0% - 30 là YES",
                PointFrom = 0,
                PointTo = 30,
                SortOrder = 1,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipLevels.Add(rela_1);

            var rela_2 = new RelationshipLevel
            {
                Id = Guid.NewGuid(),
                Code = "B",
                Description = "Là mức A và đã có 5 lần chia sẽ và nhờ vã",
                Review = "Từ 31% - 40 là YES",
                PointFrom = 31,
                PointTo = 40,
                SortOrder = 2,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipLevels.Add(rela_2);

            var rela_3 = new RelationshipLevel
            {
                Id = Guid.NewGuid(),
                Code = "C",
                Description = "Là mức B và đã có 5 lần giới thiệu khách hàng này với những người bạn quen biết",
                Review = "Từ 41% - 50 là YES",
                PointFrom = 41,
                PointTo = 50,
                SortOrder = 3,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipLevels.Add(rela_3);

            var rela_4 = new RelationshipLevel
            {
                Id = Guid.NewGuid(),
                Code = "D",
                Description = "Là mức C và đã có 2 lần giới thiếu về sản phẩm, dịch vụ của công ty",
                Review = "Từ 51% - 70 là YES",
                PointFrom = 51,
                PointTo = 70,
                SortOrder = 4,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipLevels.Add(rela_4);

            var rela_5 = new RelationshipLevel
            {
                Id = Guid.NewGuid(),
                Code = "E",
                Description = "Là mức D và đã có 2 lần khách hàng chia sẽ khó khăn",
                Review = "Từ 71% - 90 là YES",
                PointFrom = 71,
                PointTo = 90,
                SortOrder = 5,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipLevels.Add(rela_5);

            var rela_6 = new RelationshipLevel
            {
                Id = Guid.NewGuid(),
                Code = "F",
                Description = "Là mức E và đã có 1 lần góp phần làm cho cuộc sống khách hàng tốt hơn",
                Review = "Từ 91% - 100 là YES",
                PointFrom = 91,
                PointTo = 100,
                SortOrder = 6,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.RelationshipLevels.Add(rela_6);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region ContractStatus
        var ContractStatusId = Guid.NewGuid();
        if (!_context.ContractStatuses.Any())
        {
            var processing = new ContractStatus
            {
                Id = ContractStatusId,
                Code = ContractStatusEnum.PROCESSING.ToString(),
                Name = "Đang thực hiện",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.ContractStatuses.Add(processing);

            var completed = new ContractStatus
            {
                Id = Guid.NewGuid(),
                Code = ContractStatusEnum.COMPLETED.ToString(),
                Name = "Hoàn thành",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.ContractStatuses.Add(completed);

            var noContract = new ContractStatus
            {
                Id = Guid.NewGuid(),
                Code = ContractStatusEnum.NOCONTRACT.ToString(),
                Name = "Chưa có hợp đồng",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.ContractStatuses.Add(noContract);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region ProjectStatus
        var ProjectStatusId = Guid.NewGuid();
        if (!_context.ProjectStatuses.Any())
        {
            var processing = new ProjectStatus
            {
                Id = ProjectStatusId,
                Code = ProjectStatusEnum.PROCESSING.ToString(),
                Name = "Đang thực hiện",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.ProjectStatuses.Add(processing);

            var done = new ProjectStatus
            {
                Id = Guid.NewGuid(),
                Code = ProjectStatusEnum.DONE.ToString(),
                Name = "Hoàn thành",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.ProjectStatuses.Add(done);

            var todo = new ProjectStatus
            {
                Id = Guid.NewGuid(),
                Code = ProjectStatusEnum.TODO.ToString(),
                Name = "Chưa hoạt động",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.ProjectStatuses.Add(todo);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region Customer
        Guid CustomerId_1 = Guid.NewGuid();
        Guid CustomerId_2 = Guid.NewGuid();
        Guid CustomerId_3 = Guid.NewGuid();
        if (!_context.Customers.Any())
        {
            var customer_1 = new Customer
            {
                Id = CustomerId_1,
                Code = "CUSTOMER_1",
                Fullname = "Khách hàng 1",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.Customers.Add(customer_1);

            var customer_2 = new Customer
            {
                Id = CustomerId_2,
                Code = "CUSTOMER_2",
                Fullname = "Khách hàng 2",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.Customers.Add(customer_2);

            var customer_3 = new Customer
            {
                Id = CustomerId_3,
                Code = "CUSTOMER_3",
                Fullname = "Khách hàng 3",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.Customers.Add(customer_3);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region Contract
        if (!_context.Contracts.Any())
        {
            var contract_1 = new Contract
            {
                Id = Guid.NewGuid(),
                Code = "CONTRACT1",
                Number = "ABCD",
                Name = "Hợp đồng 1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(2),
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                ContractStatusId = ContractStatusId,
                CustomerId = CustomerId_1
            };
            _context.Contracts.Add(contract_1);

            var contract_2 = new Contract
            {
                Id = Guid.NewGuid(),
                Code = "CONTRACT2",
                Number = "FBSAJ",
                Name = "Hợp đồng 2",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(2),
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                ContractStatusId = ContractStatusId,
                CustomerId = CustomerId_2
            };
            _context.Contracts.Add(contract_2);

            var contract_3 = new Contract
            {
                Id = Guid.NewGuid(),
                Code = "CONTRACT3",
                Number = "WQDMQ",
                Name = "Hợp đồng 3",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(2),
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                ContractStatusId = ContractStatusId,
                CustomerId = CustomerId_3
            };
            _context.Contracts.Add(contract_3);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region Service

        if (!_context.Services.Any())
        {
            var service_1 = new Service
            {
                Id = Guid.NewGuid(),
                Code = "DICH_VU_1",
                Name = "DICH VU 1",
                ShortName = "DV1",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.Services.Add(service_1);

            var service_2 = new Service
            {
                Id = Guid.NewGuid(),
                Code = "DICH_VU_2",
                Name = "DICH VU 2",
                ShortName = "DV2",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.Services.Add(service_2);

            var service_3 = new Service
            {
                Id = Guid.NewGuid(),
                Code = "DICH_VU_3",
                Name = "DICH VU 3",
                ShortName = "DV3",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };
            _context.Services.Add(service_3);

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Project
        if (!_context.Projects.Any())
        {
            var services = await _context.Services.ToListAsync();
            var project_1 = new Project
            {
                Id = Guid.NewGuid(),
                Code = "DU_AN_1",
                Name = "Dự án 1",
                Result = "Tốt",
                Type = "Kinh doanh",
                Note = "",
                Service = services[0].Id.ToString(),
                Point = 100,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                ProjectStatusId = ProjectStatusId,
                //ApplicationUserId = testAccount.Id
            };
            _context.Projects.Add(project_1);

            var project_2 = new Project
            {
                Id = Guid.NewGuid(),
                Code = "DU_AN_2",
                Name = "Dự án 2",
                Result = "Tốt",
                Type = "Công nghệ",
                Note = "",
                Service = services[0].Id.ToString(),
                Point = 100,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                ProjectStatusId = ProjectStatusId,
                //ApplicationUserId = testAccount.Id
            };
            _context.Projects.Add(project_2);

            var project_3 = new Project
            {
                Id = Guid.NewGuid(),
                Code = "DU_AN_3",
                Name = "Dự án 3",
                Result = "Tốt",
                Type = "Kinh doanh",
                Note = "",
                Service = services[0].Id.ToString(),
                Point = 100,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                ProjectStatusId = ProjectStatusId,
                //ApplicationUserId = testAccount.Id
            };
            _context.Projects.Add(project_3);

            await _context.SaveChangesAsync();
        };
        #endregion

        #region GainsQuestions

        if (!_context.GainsQuestions.Any())
        {
            var gainsQuestions1 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 1,
                Content = "Thông tin cá nhân của khách hàng",
                Description = "Thông tin cá nhân của khách hàng",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions1);

            var gainsQuestions2 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 2,
                Content = "Thông tin kinh nghiệm của khách hàng",
                Description = "Thông tin kinh nghiệm của khách hàng",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions2);

            var gainsQuestions3 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 3,
                Content = "Thành viên gia đình",
                Description = "Thành viên gia đình",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions3);

            var gainsQuestions4 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 4,
                Content = "Mục tiêu năm tới của họ",
                Description = "Mục tiêu năm tới của họ",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions4);

            var gainsQuestions5 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 5,
                Content = "Bạn có thể gọi họ mặc dù rất muộn nếu bạn thực sự cần giúp đỡ",
                Description = "Bạn có thể gọi họ mặc dù rất muộn nếu bạn thực sự cần giúp đỡ",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions5);

            var gainsQuestions6 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 6,
                Content = "Bạn không ngần ngại khi đề nghị họ giúp về đời sống",
                Description = "Bạn không ngần ngại khi đề nghị họ giúp về đời sống",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions6);

            var gainsQuestions7 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 7,
                Content = "Bạn không ngần ngại khi đề nghị họ giúp về công việc kinh doanh",
                Description = "Bạn không ngần ngại khi đề nghị họ giúp về công việc kinh doanh",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions7);

            var gainsQuestions8 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 8,
                Content = "Bạn có cảm thấy thú vị khi dành thời gian cùng nhau (Cafe, thể thao,...)",
                Description = "Bạn có cảm thấy thú vị khi dành thời gian cùng nhau (Cafe, thể thao,...)",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions8);

            var gainsQuestions9 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 9,
                Content = "Người đó có nằm trong tâm trí của bạn khi cần giúp đỡ, khi bạn có thời gian rảnh, khi muốn quan tâm",
                Description = "Người đó có nằm trong tâm trí của bạn khi cần giúp đỡ, khi bạn có thời gian rảnh, khi muốn quan tâm",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions9);

            var gainsQuestions10 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 10,
                Content = "Bạn có thể trao đổi thẳng thắn, cởi mở việc giúp đỡ lẫn nhau hoặc người khác",
                Description = "Bạn có thể trao đổi thẳng thắn, cởi mở việc giúp đỡ lẫn nhau hoặc người khác",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions10);

            var gainsQuestions11 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 11,
                Content = "Bạn và người đó đã có những khoảnh khắc cùng nhau làm việc gì đáng nhớ",
                Description = "Bạn và người đó đã có những khoảnh khắc cùng nhau làm việc gì đáng nhớ",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions11);

            var gainsQuestions12 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 12,
                Content = "Bạn đã được đối tác chia sẻ khó khăn",
                Description = "Bạn đã được đối tác chia sẻ khó khăn",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions12);

            var gainsQuestions13 = new GainsQuestion()
            {
                Id = Guid.NewGuid(),
                Code = 13,
                Content = "Bạn đã được đối tác nhờ giúp đỡ",
                Description = "Bạn đã được đối tác nhờ giúp đỡ",
                CreatedDate = DateTime.Now
            };

            _context.GainsQuestions.Add(gainsQuestions13);

            await _context.SaveChangesAsync();
        }


        #endregion

        #region Criteria

        if (!_context.Criterias.Any())
        {
            var criteria1 = new Criteria()
            {
                Id = Guid.NewGuid(),
                Code = "CRITERIA_CUSTOMER",
                Name = "Phát triển quan hệ khách hàng"
            };

            _context.Criterias.Add(criteria1);

            var criteria2 = new Criteria()
            {
                Id = Guid.NewGuid(),
                Code = "CRITERIA_1",
                Name = "Tiêu chí 1"
            };

            _context.Criterias.Add(criteria2);

            var criteria3 = new Criteria()
            {
                Id = Guid.NewGuid(),
                Code = "CRITERIA_2",
                Name = "Tiêu chí 2"
            };

            _context.Criterias.Add(criteria3);

            await _context.SaveChangesAsync();
        }

        #endregion Criteria

        _context.ClearChangeTracker();
    }

    public async Task<int> InitMenu(List<string> listMenuActive = null)
    {
        int rows = 0;
        if (!_context.Menus.Any())
        {
            #region MenuIds
            // *********** QLTK
            Guid QLTK = Guid.NewGuid(),
            QLTK_TK = Guid.NewGuid(),
            QLTK_HS = Guid.NewGuid(),
            QLTK_QTBM = Guid.NewGuid(),
            QLTK_ROLE = Guid.NewGuid(),
            QLTK_PQ = Guid.NewGuid(),
            // *********** SALE
            Sale = Guid.NewGuid(),
            Sale_MT = Guid.NewGuid(),
            Sale_QL = Guid.NewGuid(),
            Sale_MQH = Guid.NewGuid(),
            Sale_CH = Guid.NewGuid(),
            Sale_SK = Guid.NewGuid(),
            //Sale_SK_PQ = Guid.NewGuid(),
            Sale_SK_PQ = Guid.NewGuid(),
            Sale_EL_NV = Guid.NewGuid(),
            Sale_EL_GD = Guid.NewGuid(),
            Sale_EL_NCN = Guid.NewGuid(),
            // *********** Danh Mục
            DM = Guid.NewGuid(),
            DM_KH = Guid.NewGuid(),
            DM_NS = Guid.NewGuid(),
            DM_NCC = Guid.NewGuid(),
            DM_MDV = Guid.NewGuid(),
            DM_MDQH = Guid.NewGuid(),
            DM_GAINS = Guid.NewGuid(),
            DM_HD = Guid.NewGuid(),
            DM_DA = Guid.NewGuid(),
            DM_TC = Guid.NewGuid(),
            // *********** Nhân sự
            NS = Guid.NewGuid(),
            NS_TTNS = Guid.NewGuid(),
            NS_TTTN = Guid.NewGuid(),
            NS_TTTC = Guid.NewGuid(),
            NS_BCTN = Guid.NewGuid();
            #endregion
            var isActiveSale = false;
            var isActiveNS = false;
            var isActiveDM = false;
            if (listMenuActive != null)
            {
                if (listMenuActive.Contains(MenuType.Sale))
                    isActiveSale = true;
                if (listMenuActive.Contains(MenuType.NS))
                    isActiveNS = true;
                if (listMenuActive.Contains(MenuType.DM))
                    isActiveDM = true;
            }
            _context.Menus.AddRange(new List<Menu>(){
                /*new Menu() { Id = QLTK, SortOrder = 1, Code = MenuType.QLTK, Name = "Quản lý người dùng", NameEn="User management", ParentId = new Nullable<Guid>(), Icon = "tenicon.png" },
                new Menu() { Id = QLTK_TK, SortOrder = 1, Code = MenuType.QLTK_TK, Name = "Tài khoản", NameEn="Accounts", ParentId =QLTK, Icon = "tenicon.png" },
                new Menu() { Id = QLTK_HS, SortOrder = 2, Code = MenuType.QLTK_HS, Name = "Hồ sơ", NameEn="Profiles", ParentId =QLTK, Icon = "tenicon.png" },
                new Menu() { Id = QLTK_QTBM, SortOrder = 3, Code = MenuType.QLTK_QTBM, Name = "Quy tắc bảo mật", NameEn="Security rules", ParentId = QLTK, Icon = "tenicon.png" },
                new Menu() { Id = QLTK_ROLE, SortOrder = 4, Code = MenuType.QLTK_ROLE, Name = "Chức vụ", NameEn="Roles", ParentId = QLTK, Icon = "tenicon.png" },
                new Menu() { Id = QLTK_PQ, SortOrder = 5, Code = MenuType.QLTK_PQ, Name = "Phân quyền", NameEn="Permissions", ParentId = QLTK, Icon = "tenicon.png" },*/

				// ***** Icon , NameAction , BreadcrumbNavigation , Sort dựa theo define của frontend

                new Menu() { Id = Sale, SortOrder = 1, Code = MenuType.Sale, IsActivite=isActiveSale, Name = "Sales", NameEn="Sales", ParentId = new Nullable<Guid>(), Icon = "sale", NameAction = "sales"},
                new Menu() { Id = Sale_MT, SortOrder = 1, Code = MenuType.Sale_MT, IsActivite=isActiveSale,Name = "Mục tiêu", NameEn="KPIs", ParentId = Sale, Icon = "dot" , NameAction = "kpi", BreadcrumbNavigation = "/sales/kpi"},
                new Menu() { Id = Sale_QL, SortOrder = 2, Code = MenuType.Sale_QL,IsActivite=isActiveSale, Name = "Quyền lợi", NameEn="Privileges", ParentId = Sale, Icon = "dot", NameAction = "privileges", BreadcrumbNavigation = "/sales/privileges"},
                new Menu() { Id = Sale_MQH, SortOrder = 3, Code = MenuType.Sale_MQH,IsActivite=isActiveSale, Name = "Mối quan hệ", NameEn="Relationships", ParentId = Sale, Icon = "dot", NameAction = "relationship", BreadcrumbNavigation = "/sales/relationship"},
                new Menu() { Id = Sale_CH, SortOrder = 4, Code = MenuType.Sale_CH, IsActivite=isActiveSale,Name = "Cơ hội", NameEn="Opportunities", ParentId = Sale, Icon = "dot", NameAction = "opportunity", BreadcrumbNavigation = "/sales/opportunity"},
                new Menu() { Id = Sale_SK, SortOrder = 5, Code = MenuType.Sale_SK, IsActivite=isActiveSale,Name = "Sale kit", NameEn="Salekit", ParentId = Sale, Icon = "dot", NameAction = "sale-kit", BreadcrumbNavigation = "/sales/sale-kit"},
                new Menu() { Id = Sale_EL_NV, SortOrder = 6, Code = MenuType.Sale_EL_NV, IsActivite=isActiveSale, Name = "Tự học", NameEn="E-Learning", ParentId = Sale, Icon = "dot", NameAction = "e-learning", BreadcrumbNavigation = "/sales/elearning"},
                new Menu() { Id = Sale_EL_GD, SortOrder = 8, Code = MenuType.Sale_EL_GD, IsActivite=isActiveSale, Name = "Quản lý chương trình học", NameEn="Syllabus Management", ParentId = Sale, Icon = "dot", NameAction = "syllabus-management", BreadcrumbNavigation = "/sales/syllabus-management"},
                new Menu() { Id = Sale_EL_NCN, SortOrder = 7, Code = MenuType.Sale_EL_NCN, IsActivite=isActiveSale, Name = "Quản lý khóa học", NameEn="Unit Management", ParentId = Sale, Icon = "dot", NameAction = "unit-management", BreadcrumbNavigation = "/sales/unit-management"},
	
				//new Menu() { Id = Sale_SK_PQ, SortOrder = 6, Code = MenuType.Sale_SK_PQ, Name = "Phân quyền Sale kit", NameEn="Sale kit Permissions", ParentId = Sale, Icon = "dot"},

				new Menu() { Id = NS, SortOrder = 2, Code = MenuType.NS,IsActivite=isActiveNS, Name = "Nhân sự", NameEn="Personnel", ParentId = new Nullable<Guid>(), Icon = "user-group", NameAction = "personnel" },
                new Menu() { Id = NS_TTNS, SortOrder = 1, Code = MenuType.NS_TTNS,IsActivite=isActiveNS, Name = "Thông tin nhân sự", NameEn="Users information", ParentId = NS, Icon = "dot" , NameAction = "human-resources", BreadcrumbNavigation = "/personnel/human-resources"},
                new Menu() { Id = NS_TTTN, SortOrder = 2, Code = MenuType.NS_TTTN, IsActivite=isActiveNS,Name = "Thông tin thu nhập", NameEn="Income information", ParentId = NS, Icon = "dot" , NameAction = "income", BreadcrumbNavigation = "/personnel/income" },

                new Menu() { Id = DM, SortOrder = 3, Code = MenuType.DM, IsActivite=isActiveDM,Name = "Danh mục", NameEn="Categories", ParentId = new Nullable<Guid>(), Icon = "category", NameAction = "category"  },
                new Menu() { Id = DM_KH, SortOrder = 1, Code = MenuType.DM_KH,IsActivite=isActiveDM, Name = "Khách hàng", NameEn="Customers", ParentId = DM, Icon = "dot", NameAction = "customer", BreadcrumbNavigation = "/category/customer"},
                new Menu() { Id = DM_NS, SortOrder = 2, Code = MenuType.DM_NS, IsActivite=isActiveDM,Name = "Nhân sự", NameEn="Human resources", ParentId =DM, Icon = "dot", NameAction = "human-resource", BreadcrumbNavigation = "/category/human-resource"},
                new Menu() { Id = DM_NCC, SortOrder = 3, Code = MenuType.DM_NCC,IsActivite=isActiveDM, Name = "Nhà cung cấp", NameEn="Suppliers", ParentId = DM, Icon = "dot", NameAction = "supplier", BreadcrumbNavigation = "/category/supplier" },
                new Menu() { Id = DM_MDV, SortOrder = 4, Code = MenuType.DM_MDV,IsActivite=isActiveDM, Name = "Mảng dịch vụ", NameEn="Services", ParentId = DM, Icon = "dot", NameAction = "service", BreadcrumbNavigation = "/category/service" },
                new Menu() { Id = DM_MDQH, SortOrder = 5, Code = MenuType.DM_MDQH,IsActivite=isActiveDM, Name = "Mức độ quan hệ", NameEn="Relationship level", ParentId = DM, Icon = "dot", NameAction = "relationship", BreadcrumbNavigation = "/category/relationship" },
                new Menu() { Id = DM_GAINS, SortOrder = 6, Code = MenuType.DM_GAINS, IsActivite=isActiveDM,Name = "Câu hỏi bảng GAINS", NameEn="GAINS board questions", ParentId = DM, Icon = "dot", NameAction = "questions", BreadcrumbNavigation = "/category/questions" },
                new Menu() { Id = DM_HD, SortOrder = 7, Code = MenuType.DM_HD,IsActivite=isActiveDM, Name = "Hợp đồng", NameEn="Contracts", ParentId = DM, Icon = "dot", NameAction = "contract", BreadcrumbNavigation = "/category/contract"},
                new Menu() { Id = DM_DA, SortOrder = 8, Code = MenuType.DM_DA,IsActivite=isActiveDM, Name = "Dự án", NameEn="Projects", ParentId = DM, Icon = "dot", NameAction = "project", BreadcrumbNavigation = "/category/project"},
                 new Menu() { Id = DM_TC, SortOrder = 9, Code = MenuType.DM_TC,IsActivite=isActiveDM, Name = "Tiêu chí", NameEn="Criterias", ParentId = DM, Icon = "dot", NameAction = "criteria", BreadcrumbNavigation = "/category/criteria"},

				/*new Menu() { Id = NS_TTTC, SortOrder = 3, Code = MenuType.NS_TTTC, Name = "Thông tin tổ chức", NameEn="Organization information", ParentId = NS, Icon = "dot" },
                new Menu() { Id = NS_BCTN, SortOrder = 4, Code = MenuType.NS_BCTN, Name = "Báo cáo nhân sự", NameEn="Human resources report", ParentId = NS, Icon = "tenicon.png" }*/
            });

            rows = rows + await _context.SaveChangesAsync();
        }
        return rows;
    }

    public async Task<int> InitFeatureMenu()
    {
        int added = 0;
        var menus = await _context.Menus.ToListAsync();
        if (_context.Menus.Any())
        {
            // Init FeatureMenu thì không cần truyền RolePositionEnum để có thể config quyền cho tửng menu
            #region MODULE_SALE
            var features = await _context.Features.ToListAsync();
            //var Sale_SK_PQ = menus.Where(s => s.Code == MenuType.Sale_SK_PQ).FirstOrDefault();
            //if (Sale_SK_PQ != null)
            //{
            //	var lst = FeatureDataConstant.sale_phanquyen_salekit_feature();
            //	var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
            //	_context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, Sale_SK_PQ));
            //}

            var Sale_SK = menus.Where(s => s.Code == MenuType.Sale_SK).FirstOrDefault();
            if (Sale_SK != null)
            {
                var lst = FeatureDataConstant.sale_salekit_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, Sale_SK));
            }

            var Sale_QL = menus.Where(s => s.Code == MenuType.Sale_QL).FirstOrDefault();
            if (Sale_QL != null)
            {
                var lst = FeatureDataConstant.sale_ql_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, Sale_QL));
            }

            var Sale_MT = menus.Where(s => s.Code == MenuType.Sale_MT).FirstOrDefault();
            if (Sale_MT != null)
            {
                var lst = FeatureDataConstant.sale_mt_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, Sale_MT));
            }

            var Sale_MQH = menus.Where(s => s.Code == MenuType.Sale_MQH).FirstOrDefault();
            if (Sale_MQH != null)
            {
                var lst = FeatureDataConstant.sale_mqh_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, Sale_MQH));
            }

            var Sale_CH = menus.Where(s => s.Code == MenuType.Sale_CH).FirstOrDefault();
            if (Sale_CH != null)
            {
                var lst = FeatureDataConstant.sale_cohoi_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, Sale_CH));
            }

            var sale_EL_NV = menus.Where(s => s.Code == MenuType.Sale_EL_NV).FirstOrDefault();
            if (sale_EL_NV != null)
            {
                var lst = FeatureDataConstant.sale_elearning_feature(RolePositionEnum.EMPLOYEE);
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, sale_EL_NV));
            }
            var sale_EL_GD = menus.Where(s => s.Code == MenuType.Sale_EL_GD).FirstOrDefault();
            if (sale_EL_GD != null)
            {
                var lst = FeatureDataConstant.sale_elearning_feature(RolePositionEnum.MANAGER);
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, sale_EL_GD));
            }
            var sale_EL_NCN = menus.Where(s => s.Code == MenuType.Sale_EL_NCN).FirstOrDefault();
            if (sale_EL_NCN != null)
            {
                var lst = FeatureDataConstant.sale_elearning_feature(RolePositionEnum.ADMINISTRATOR);
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, sale_EL_NCN));
            }

            #endregion

            #region MODULE_NHAN_SU
            var lst_nhansu = new List<string>()
            {
                MenuType.NS_TTNS, //MenuType.NS_TTTC, , MenuType.NS_BCTN
			};
            var NhanSus = menus.Where(s => s.Code != null && lst_nhansu.Contains(s.Code)).ToList();
            foreach (var item in NhanSus)
            {
                var lst = FeatureDataConstant.nhanhsu_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, item));
            }

            var ThuNhaps = menus.Where(s => s.Code == MenuType.NS_TTTN).FirstOrDefault();
            if (ThuNhaps != null)
            {
                var lst = FeatureDataConstant.thu_nhap_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, ThuNhaps));
            }
            #endregion

            #region MODULE_DANH_MUC
            var lst_danhmuc = new List<string>()
            {
                MenuType.DM_NCC, MenuType.DM_MDV, MenuType.DM_MDQH , MenuType.DM_KH , MenuType.DM_HD , MenuType.DM_GAINS, MenuType.DM_TC
            };
            var DanhMucs = menus.Where(s => s.Code != null && lst_danhmuc.Contains(s.Code)).ToList();
            foreach (var item in DanhMucs)
            {
                var lst = FeatureDataConstant.common_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, item));
            }

            var DanhMucNhanSus = menus.Where(s => s.Code == MenuType.DM_NS).FirstOrDefault();
            if (DanhMucNhanSus != null)
            {
                var lst = FeatureDataConstant.danhmuc_nhansu_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, DanhMucNhanSus));
            }

            //*************** Dự Án
            var DuAns = menus.Where(s => s.Code == MenuType.DM_DA).FirstOrDefault();
            if (DuAns != null)
            {
                var lst = FeatureDataConstant.danhmuc_du_an_feature();
                var tmp_features = features.Where(s => lst.Contains(s.Id)).ToList();
                _context.FeatureMenus.AddRange(getFeatureMenus(tmp_features, DuAns));
            }

            #endregion


            _context.Menus.UpdateRange(menus);
            added = await _context.SaveChangesAsync();

        }
        return added;
    }

    private async Task<bool> AddNewListPermision( List<FeatureMenu> menuCreate, string permision)
    {
        var menuCreateIds = menuCreate.Select(s => s.Id).ToList();

        // Check duplicate
        var rolePositionFeatureMenuExist = await _context.RolePositionFeatureMenus.Where(s => s.RolePositionId == permision
                                                                                           && s.FeatureMenuId != null
                                                                                           && menuCreateIds.Contains(s.FeatureMenuId.Value))
                                                                                  .ToListAsync();

        if (rolePositionFeatureMenuExist == null || rolePositionFeatureMenuExist.Count == 0)
        {
            var listCreateRolePositionFMByRolePositionIds = menuCreate
                                                                    .Select(s => new RolePositionFeatureMenu
                                                                    {
                                                                        RolePositionId = permision,
                                                                        FeatureMenuId = s.Id
                                                                    })
                                                                    .ToList();

            if (listCreateRolePositionFMByRolePositionIds != null && listCreateRolePositionFMByRolePositionIds.Count > 0)
            {
                _context.RolePositionFeatureMenus.AddRange(listCreateRolePositionFMByRolePositionIds);
            }
        }

        // Add new - ApplicationRoleDetails
        var positionCreates = await _context.ApplicationRoles.Where(s => s.RolePositionId == permision)
                                                       .Select(x => x.Id)
                                                       .ToListAsync();

        var listMenuCreateByRolePosition = menuCreate
                                                    .SelectMany(menu => positionCreates
                                                        .Select(position => new ApplicationRoleDetail
                                                        {
                                                            MenuId = menu.MenuId,
                                                            FeatureId = menu.FeatureId,
                                                            ApplicationRoleId = position
                                                        }))
                                                    .ToList();

        if (rolePositionFeatureMenuExist == null || rolePositionFeatureMenuExist.Count == 0)
        {
            _context.ApplicationRoleDetails.AddRange(listMenuCreateByRolePosition);
        }
        return true;
    }
    private async Task<bool> AddNewPermision(string sConnect, List<FeatureMenu> featureMenus)
    {
        if (!string.IsNullOrEmpty(sConnect)) _context.CurrentTenantConnectionString = sConnect;
        var menuUpdate=await _context.Menus.Where(x=>x.Code== "DM_GAINS" && x.NameAction!= "questions").FirstOrDefaultAsync();
        if (menuUpdate != null)
        {
            menuUpdate.NameAction = "questions";
        }
        // Add new - RolePositionFeatureMenus EMPLOYEE
        var menuCreate = featureMenus.Where(s => s.Menu != null && s.Menu.Code != null
                                            && s.Menu.Code.StartsWith(MenuType.Sale_MT.ToString())
                                                   && (s.FeatureId == FeatureType.MT_DEXUATCHINHSUA.ToString()))
                                     .ToList();
        await AddNewListPermision(menuCreate, RolePositionEnum.EMPLOYEE.ToString());
        

        var menuCreate1 = featureMenus.Where(s => s.Menu != null && s.Menu.Code != null
                                            && s.Menu.Code.StartsWith(MenuType.Sale_QL.ToString())
                                                   && (s.FeatureId == FeatureType.DELETE.ToString()))
                                     .ToList();
        await AddNewListPermision(menuCreate1, RolePositionEnum.MANAGER.ToString());
        await AddNewListPermision(menuCreate1, RolePositionEnum.ADMINISTRATOR.ToString());

        var menuCreate2 = featureMenus.Where(s => s.Menu != null && s.Menu.Code != null
                                            && s.Menu.Code.StartsWith(MenuType.Sale_QL.ToString())
                                                   && (s.FeatureId == FeatureType.QL_HISTORY.ToString()))
                                     .ToList();
        await AddNewListPermision(menuCreate2, RolePositionEnum.EMPLOYEE.ToString());
        await AddNewListPermision(menuCreate2, RolePositionEnum.ADMINISTRATOR.ToString());
        return true;
    }
    public async Task<bool> RemoveRolePositionFeatureMenu(string sConnect)
    {
        if (!string.IsNullOrEmpty(sConnect)) _context.CurrentTenantConnectionString = sConnect;

        var featureMenus = await _context.FeatureMenus.Include(s => s.Menu)
                                                      .Include(s => s.Feature)
                                                      .ToListAsync();

        var commonMenu = featureMenus.Where(s => s.Menu != null && s.Menu.Code != null
                                                && (
                                                      (s.Menu.Code.StartsWith(MenuType.NS_TTNS.ToString())
                                                             && (s.FeatureId == FeatureType.IMPORT_EXCEL.ToString()
                                                              || s.FeatureId == FeatureType.EXPORT_EXCEL.ToString()
                                                              || s.FeatureId == FeatureType.MYSELF.ToString()))
                                                   || (s.Menu.Code.StartsWith("DM_")
                                                             && (s.FeatureId == FeatureType.IMPORT_EXCEL.ToString()
                                                              || s.FeatureId == FeatureType.EXPORT_EXCEL.ToString()
                                                              || s.FeatureId == FeatureType.MYSELF.ToString()
                                                              || s.FeatureId == FeatureType.DETAIL.ToString()))
                                                   || (s.Menu.Code.StartsWith(MenuType.NS_TTTN.ToString())
                                                             && (s.FeatureId == FeatureType.EXPORT_EXCEL.ToString()))
                                                   || (s.Menu.Code.StartsWith(MenuType.Sale_MT.ToString())
                                                             && (s.FeatureId == FeatureType.IMPORT_EXCEL.ToString()
                                                              || s.FeatureId == FeatureType.EXPORT_EXCEL.ToString()
                                                              || s.FeatureId == FeatureType.DETAIL.ToString()))                                                        
                                                   || (s.Menu.Code.StartsWith(MenuType.Sale_MQH.ToString())
                                                                        && s.FeatureId == FeatureType.DETAIL.ToString())
                                                    || (s.Menu.Code.StartsWith(MenuType.Sale_QL.ToString())
                                                                        && s.FeatureId == FeatureType.DETAIL.ToString())
                                                   )
                                           )
                                      .ToList();

        var listFmId = commonMenu.Select(x => x.Id).ToList();

        var roleFM = await _context.RolePositionFeatureMenus.Where(x => listFmId.Contains(x.FeatureMenuId.Value))
                                                            .ToListAsync();
        if (roleFM != null)
        {
            _context.RolePositionFeatureMenus.RemoveRange(roleFM);
        }
        // Add new - RolePositionFeatureMenus 
        await AddNewPermision(sConnect, featureMenus);
        // RolePositionFeatureMenu - Remove feature has RolePositionId equals EMPLOYEE
        #region "Remove RolePositionFeatureMenus equals EMPLOYEE"
        var commonMenuByRolePosition = featureMenus.Where(s => s.Menu != null && s.Menu.Code != null &&
                                                            (
                                                                (s.Menu.Code.StartsWith(MenuType.Sale_SK.ToString())
                                                                        && s.FeatureId == FeatureType.SALEKIT_XEMPHANQUYEN.ToString())
                                                             || (s.Menu.Code.StartsWith(MenuType.NS_TTTN.ToString())
                                                                        && s.FeatureId == FeatureType.IMPORT_EXCEL.ToString())
                                                             || (s.Menu.Code.StartsWith(MenuType.Sale_MT.ToString())
                                                                        && (s.FeatureId == FeatureType.MT_CHOTDIEUCHINH.ToString()
                                                                        || s.FeatureId == FeatureType.MT_XEMDEXUAT.ToString()))
                                                             || (s.Menu.Code.StartsWith(MenuType.Sale_CH.ToString())
                                                                        && s.FeatureId == FeatureType.DETAIL.ToString())
                                                                        || (s.Menu.Code.StartsWith(MenuType.Sale_MQH.ToString())
                                                                        && s.FeatureId == FeatureType.DETAIL.ToString())
                                                             || (s.Menu.Code.StartsWith(MenuType.Sale_MQH.ToString())
                                                                        && (s.FeatureId == FeatureType.DETAIL.ToString()))
                                                            || (s.Menu.Code.StartsWith(MenuType.Sale_QL.ToString())
                                                                        && (s.FeatureId == FeatureType.DETAIL.ToString()
                                                                        || s.FeatureId == FeatureType.QL_XEMDEXUAT.ToString()
                                                                        || s.FeatureId == FeatureType.QL_CHOTDIEUCHINH.ToString()))
                                                            )
                                                         )
                                                   .ToList();

        var listFMByRolePositionIds = commonMenuByRolePosition.Select(x => x.Id).ToList();

        var roleFMByRolePosition = await _context.RolePositionFeatureMenus.Where(x => x.RolePositionId == RolePositionEnum.EMPLOYEE.ToString()
                                                                      && listFMByRolePositionIds.Contains(x.FeatureMenuId.Value))
                                                                          .ToListAsync();

        if (roleFMByRolePosition != null)
        {
            _context.RolePositionFeatureMenus.RemoveRange(roleFMByRolePosition);
        }
        #endregion
        // RolePositionFeatureMenu - Remove feature has RolePositionId equals MANAGER
        #region "Remove RolePositionFeatureMenus equals MANAGER"
        var commonMenuByRolePositionManager = featureMenus.Where(s => s.Menu != null && s.Menu.Code != null &&
                                                                    (
                                                                        (s.Menu.Code.StartsWith(MenuType.Sale_MT.ToString())
                                                                                && s.FeatureId == FeatureType.MT_CAPNHATDEXUAT.ToString())
                                                                     || (s.Menu.Code.StartsWith(MenuType.Sale_CH.ToString())
                                                                                && (s.FeatureId == FeatureType.DETAIL.ToString()
                                                                                        || s.FeatureId == FeatureType.IMPORT_EXCEL.ToString()
                                                                                        || s.FeatureId == FeatureType.EXPORT_EXCEL.ToString()
                                                                                        || s.FeatureId == FeatureType.MYSELF.ToString()))
                                                                       || (s.Menu.Code.StartsWith(MenuType.Sale_MQH.ToString())
                                                                               && (s.FeatureId == FeatureType.DETAIL.ToString()))
                                                                        || (s.Menu.Code.StartsWith(MenuType.Sale_QL.ToString())
                                                                        && (s.FeatureId == FeatureType.DETAIL.ToString()
                                                                        || s.FeatureId == FeatureType.QL_CAPNHATDEXUAT.ToString()))

                                                                    )
                                                                 )
                                                           .ToList();

        var listFMByRolePositionManagerIds = commonMenuByRolePositionManager.Select(x => x.Id).ToList();

        var roleFMByRolePositionManager = await _context.RolePositionFeatureMenus.Where(x => x.RolePositionId == RolePositionEnum.MANAGER.ToString()
                                                                                          && listFMByRolePositionManagerIds.Contains(x.FeatureMenuId.Value))
                                                                                 .ToListAsync();

        if (roleFMByRolePositionManager != null)
        {
            _context.RolePositionFeatureMenus.RemoveRange(roleFMByRolePositionManager);
        }
        #endregion
        // ApplicationRoleDetail - Remove permision all
        #region "Remove ApplicationRoleDetail All"
        var permission = await _context.ApplicationRoleDetails
                                      .Where(s => s.DeleteFlag != true && s.FeatureId != null && s.MenuId != null)
                                      .ToListAsync();

        var listMenu = commonMenu.Select(x => new { Id = x.MenuId, FM = x.FeatureId })
                                 .ToList();

        var permisionRemove = permission.Where(x => listMenu.Any(y => y.Id == x.MenuId && y.FM == x.FeatureId))
                                        .ToList();

        if (permisionRemove != null)
        {
            _context.ApplicationRoleDetails.RemoveRange(permisionRemove);
        }
        #endregion
        // ApplicaitonRoleDetail - Remove permission has RolePositionId = EMPLOYEE
        #region "Remove ApplicationRoleDetail EMPLOYEE"
        var positions = await _context.ApplicationRoles.Where(s => s.RolePositionId == RolePositionEnum.EMPLOYEE.ToString())
                                                       .Select(x => x.Id)
                                                       .ToListAsync();

        var listMenuByRolePosition = commonMenuByRolePosition.Select(x => new { Id = x.MenuId, FM = x.FeatureId })
                                                             .ToList();

        var permissionRemoveByRolePosition = permission.Where(x => listMenuByRolePosition.Any(y => y.Id == x.MenuId && y.FM == x.FeatureId)
                                                  && positions.Any(y => y == x.ApplicationRoleId))
                                                       .ToList();

        if (permissionRemoveByRolePosition != null)
        {
            _context.ApplicationRoleDetails.RemoveRange(permissionRemoveByRolePosition);
        }
        #endregion
        // ApplicaitonRoleDetail - Remove permission has RolePositionId = MANAGER
        #region "Remove ApplicationRoleDetail MANAGER"
        var positionsManager = await _context.ApplicationRoles.Where(s => s.RolePositionId == RolePositionEnum.MANAGER.ToString())
                                                              .Select(x => x.Id)
                                                              .ToListAsync();

        var listMenuByRolePositionManager = commonMenuByRolePositionManager.Select(x => new { Id = x.MenuId, FM = x.FeatureId })
                                                                           .ToList();

        var permissionRemoveByRolePositionManager = permission.Where(x => listMenuByRolePositionManager.Any(y => y.Id == x.MenuId && y.FM == x.FeatureId)
                                                                       && positionsManager.Any(y => y == x.ApplicationRoleId))
                                                              .ToList();

        if (permissionRemoveByRolePositionManager != null)
        {
            _context.ApplicationRoleDetails.RemoveRange(permissionRemoveByRolePositionManager);
        }
        #endregion
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> InitRolePositionFeatureMenu()
    {
        var rows = 0;
        if (!_context.RolePositionFeatureMenus.Any())
        {
            var featureMenus = await _context.FeatureMenus.Include(s => s.Menu).Include(s => s.Feature).ToListAsync();
            var positions = await _context.RolePositions.Where(s => s.IsModified != false).ToListAsync();
            var menus = await _context.Menus.ToListAsync();

            var manager = positions.Where(s => s.Id == RolePositionEnum.MANAGER.ToString()).FirstOrDefault();
            var employee = positions.Where(s => s.Id == RolePositionEnum.EMPLOYEE.ToString()).FirstOrDefault();
            var administrator = positions.Where(s => s.Id == RolePositionEnum.ADMINISTRATOR.ToString()).FirstOrDefault();
            #region Common
            var saleMenu = new List<string>()
            {
                MenuType.Sale_MT , MenuType.Sale_QL , MenuType.Sale_MQH, MenuType.Sale_CH
            };
            // Init data của những Menu không thuộc module SALE
            var commonMenu = featureMenus.Where(s => s.Menu != null && s.Menu.Code != null && !saleMenu.Contains(s.Menu.Code)).ToList();
            var common_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
            foreach (var position in positions)
            {
                foreach (var item in commonMenu)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = position.Id
                    };
                    common_RolePositionFeatureMenu.Add(data);
                }
            }
            _context.RolePositionFeatureMenus.AddRange(common_RolePositionFeatureMenu);
            #endregion

            // Đối với module sale sẽ có những quyền riêng theo từng RolePosition nên cần truyền RolePositionId
            // *** ex: Đối với mục tiêu "MANAGER" sẽ có permission "Chốt" còn "EMPLOYEE" sẽ không có ...
            // *** ex: Đối với quyền lợi "EMPLOYEE" sẽ có permission "Yêu cầu chỉnh sửa" còn "MANAGER" sẽ không có ...
            #region Sale_Muc_Tieu
            //*********** Manager
            if (manager != null)
            {
                var mucTieuFeatures = FeatureDataConstant.sale_mt_feature(RolePositionEnum.MANAGER);
                var mucTieuMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_MT &&
                                                           mucTieuFeatures.Contains(s.FeatureId!)).ToList();
                var mucTieu_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in mucTieuMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = manager.Id
                    };
                    mucTieu_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(mucTieu_RolePositionFeatureMenu);
            }
            //*********** Employee
            if (employee != null)
            {
                var mucTieuFeatures = FeatureDataConstant.sale_mt_feature(RolePositionEnum.EMPLOYEE);
                var mucTieuMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_MT &&
                                                           mucTieuFeatures.Contains(s.FeatureId!)).ToList();
                var mucTieu_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in mucTieuMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = employee.Id
                    };
                    mucTieu_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(mucTieu_RolePositionFeatureMenu);
            }
            //*********** Administrator
            if (administrator != null)
            {
                var mucTieuFeatures = FeatureDataConstant.sale_mt_feature(RolePositionEnum.ADMINISTRATOR);
                var mucTieuMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_MT &&
                                                           mucTieuFeatures.Contains(s.FeatureId!)).ToList();
                var mucTieu_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in mucTieuMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = administrator.Id
                    };
                    mucTieu_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(mucTieu_RolePositionFeatureMenu);
            }
            #endregion

            #region Sale_Quyen_Loi
            //*********** Manager
            if (manager != null)
            {
                var quyenLoiFeatures = FeatureDataConstant.sale_ql_feature(RolePositionEnum.MANAGER);
                var quyenLoiMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_QL &&
                                                           quyenLoiFeatures.Contains(s.FeatureId!)).ToList();
                var quyenLoi_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in quyenLoiMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = manager.Id
                    };
                    quyenLoi_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(quyenLoi_RolePositionFeatureMenu);
            }
            //*********** Employee
            if (employee != null)
            {
                var quyenLoiFeatures = FeatureDataConstant.sale_ql_feature(RolePositionEnum.EMPLOYEE);
                var quyenLoiMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_QL &&
                                                           quyenLoiFeatures.Contains(s.FeatureId!)).ToList();
                var quyenLoi_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in quyenLoiMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = employee.Id
                    };
                    quyenLoi_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(quyenLoi_RolePositionFeatureMenu);
            }
            //*********** Employee
            if (administrator != null)
            {
                var quyenLoiFeatures = FeatureDataConstant.sale_ql_feature(RolePositionEnum.ADMINISTRATOR);
                var quyenLoiMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_QL &&
                                                           quyenLoiFeatures.Contains(s.FeatureId!)).ToList();
                var quyenLoi_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in quyenLoiMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = administrator.Id
                    };
                    quyenLoi_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(quyenLoi_RolePositionFeatureMenu);
            }
            #endregion

            #region Sale_Moi_Quan_He
            //*********** Manager
            if (manager != null)
            {
                var moiQuanHeFeatures = FeatureDataConstant.sale_mqh_feature(RolePositionEnum.MANAGER);
                var moiQuanHeMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_MQH &&
                                                           moiQuanHeFeatures.Contains(s.FeatureId!)).ToList();
                var moiQuanHe_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in moiQuanHeMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = manager.Id
                    };
                    moiQuanHe_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(moiQuanHe_RolePositionFeatureMenu);
            }
            //*********** Employee
            if (employee != null)
            {
                var moiQuanHeFeatures = FeatureDataConstant.sale_mqh_feature(RolePositionEnum.EMPLOYEE);
                var moiQuanHeMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_MQH &&
                                                           moiQuanHeFeatures.Contains(s.FeatureId!)).ToList();
                var moiQuanHe_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in moiQuanHeMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = employee.Id
                    };
                    moiQuanHe_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(moiQuanHe_RolePositionFeatureMenu);
            }
            //*********** Employee
            if (administrator != null)
            {
                var moiQuanHeFeatures = FeatureDataConstant.sale_mqh_feature(RolePositionEnum.ADMINISTRATOR);
                var moiQuanHeMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_MQH &&
                                                           moiQuanHeFeatures.Contains(s.FeatureId!)).ToList();
                var moiQuanHe_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in moiQuanHeMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = administrator.Id
                    };
                    moiQuanHe_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(moiQuanHe_RolePositionFeatureMenu);
            }
            #endregion

            #region Sale_Co_Hoi
            //*********** Manager
            if (manager != null)
            {
                var coHoiFeatures = FeatureDataConstant.sale_cohoi_feature(RolePositionEnum.MANAGER);
                var coHoiMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_CH &&
                                                           coHoiFeatures.Contains(s.FeatureId!)).ToList();
                var coHoi_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in coHoiMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = manager.Id
                    };
                    coHoi_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(coHoi_RolePositionFeatureMenu);
            }
            //*********** Employee
            if (employee != null)
            {
                var coHoiFeatures = FeatureDataConstant.sale_cohoi_feature(RolePositionEnum.EMPLOYEE);
                var coHoiMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_CH &&
                                                           coHoiFeatures.Contains(s.FeatureId!)).ToList();
                var coHoi_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in coHoiMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = employee.Id
                    };
                    coHoi_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(coHoi_RolePositionFeatureMenu);
            }
            #endregion

            #region Sale_Elearning
            //*********** Administrator
            if (administrator != null)
            {
                var elFeatures = FeatureDataConstant.sale_elearning_feature(RolePositionEnum.ADMINISTRATOR);
                var elMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_EL_NCN &&
                                                           elFeatures.Contains(s.FeatureId!)).ToList();
                var el_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in elMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = manager.Id
                    };
                    el_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(el_RolePositionFeatureMenu);
            }
            //*********** Manager
            if (manager != null)
            {
                var elFeatures = FeatureDataConstant.sale_elearning_feature(RolePositionEnum.MANAGER);
                var elMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_EL_GD &&
                                                           elFeatures.Contains(s.FeatureId!)).ToList();
                var el_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in elMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = manager.Id
                    };
                    el_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(el_RolePositionFeatureMenu);
            }
            //*********** Employee
            if (employee != null)
            {
                var elFeatures = FeatureDataConstant.sale_elearning_feature(RolePositionEnum.EMPLOYEE);
                var elMenus = featureMenus.Where(s => s.Menu != null && s.Feature != null &&
                                                           s.Menu.Code == MenuType.Sale_EL_NV &&
                                                           elFeatures.Contains(s.FeatureId!)).ToList();
                var el_RolePositionFeatureMenu = new List<RolePositionFeatureMenu>();
                foreach (var item in elMenus)
                {
                    RolePositionFeatureMenu data = new RolePositionFeatureMenu()
                    {
                        Id = Guid.NewGuid(),
                        FeatureMenuId = item.Id,
                        RolePositionId = employee.Id
                    };
                    el_RolePositionFeatureMenu.Add(data);
                }
                _context.RolePositionFeatureMenus.AddRange(el_RolePositionFeatureMenu);
            }
            #endregion

            rows += await _context.SaveChangesAsync();
        }
        return rows;
    }

    public async Task<int> InitFeature()
    {
        int rows = 0;
        if (!_context.Features.Any())
        {
            var features = ListFeatureType.Data();
            _context.Features.AddRange(features);

            rows += await _context.SaveChangesAsync();
        }
        return rows;
    }

    private async Task<Guid> getMenuId(string code, List<Menu>? menus = null)
    {
        if (menus != null && menus.Any())
        {
            return menus.Where(s => s.Code == code).Select(s => s.Id).FirstOrDefault();
        }
        return await _context.Menus.Where(s => s.Code == code).Select(s => s.Id).FirstOrDefaultAsync();
    }

    public async Task<int> InitApplicationRoleDetail()
    {
        var m_menus = await _context.Menus.ToListAsync();
        var roles = await _context.ApplicationRoles.ToListAsync();
        var SaleDirector = roles.Where(s => s.Name == "SaleDirector").FirstOrDefault();
        var Sale = roles.Where(s => s.Name == "Sale").FirstOrDefault();
        var Supplier = roles.Where(s => s.Name == "Supplier").FirstOrDefault();
        var Administrator = roles.Where(s => s.Name == "Administrator").FirstOrDefault();
        var Admin = roles.Where(s => s.Name == "Admin").FirstOrDefault();

        #region DanhMuc
        var lstDanhMucCode = new List<string>()
        {
            MenuType.DM_NCC ,MenuType.DM_MDV, MenuType.DM_MDQH , MenuType.DM_KH ,MenuType.DM_HD , MenuType.DM_GAINS
        };
        var lstDanhMuc = _context.Menus.Include(s => s.FeatureMenus).Where(s => s.Code != null && lstDanhMucCode.Contains(s.Code)).ToList();
        foreach (var dm in lstDanhMuc)
        {
            foreach (var fea in dm.FeatureMenus)
            {
                var details = new List<ApplicationRoleDetail>()
                {
                    new ApplicationRoleDetail(){ Id = Guid.NewGuid() , MenuId = dm.Id, ApplicationRoleId = SaleDirector.Id, FeatureId = fea.FeatureId },
                    new ApplicationRoleDetail(){ Id = Guid.NewGuid() , MenuId = dm.Id, ApplicationRoleId = Administrator.Id, FeatureId = fea.FeatureId },
                    new ApplicationRoleDetail(){ Id = Guid.NewGuid() , MenuId = dm.Id, ApplicationRoleId = Admin.Id, FeatureId = fea.FeatureId }
                };
                _context.ApplicationRoleDetails.AddRange(details);

                if (fea.FeatureId == FeatureType.SEARCH)
                {
                    var tmp = new List<ApplicationRoleDetail>()
                    {
                        new ApplicationRoleDetail(){ Id = Guid.NewGuid() , MenuId = dm.Id, ApplicationRoleId = Sale.Id, FeatureId = fea.FeatureId },
                        new ApplicationRoleDetail(){ Id = Guid.NewGuid() , MenuId = dm.Id, ApplicationRoleId = Supplier.Id, FeatureId = fea.FeatureId }
                    };
                    _context.ApplicationRoleDetails.AddRange(tmp);
                }
            }
        }
        // ****************** danh mục dự án **************
        if (SaleDirector != null)
        {
            var danhMucDuAnPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.danhmuc_du_an_feature(RolePositionEnum.MANAGER))
            {
                danhMucDuAnPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.DM_DA, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(danhMucDuAnPermission);
        }
        if (Administrator != null)
        {
            var danhMucDuAnPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.danhmuc_du_an_feature(RolePositionEnum.ADMINISTRATOR))
            {
                danhMucDuAnPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.DM_DA, m_menus), ApplicationRoleId = Administrator.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(danhMucDuAnPermission);
        }
        if (Supplier != null)
        {
            var danhMucDuAnPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.danhmuc_du_an_feature(RolePositionEnum.EMPLOYEE))
            {
                danhMucDuAnPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.DM_DA, m_menus), ApplicationRoleId = Supplier.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(danhMucDuAnPermission);
        }
        if (Sale != null)
        {
            var danhMucDuAnPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.danhmuc_du_an_feature(RolePositionEnum.EMPLOYEE))
            {
                danhMucDuAnPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.DM_DA, m_menus), ApplicationRoleId = Sale.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(danhMucDuAnPermission);
        }
        // ****************** danh mục nhân sự **************
        if (SaleDirector != null)
        {
            var danhMucNhanSuPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.danhmuc_nhansu_feature(RolePositionEnum.MANAGER))
            {
                danhMucNhanSuPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.DM_NS, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(danhMucNhanSuPermission);
        }
        if (Administrator != null)
        {
            var danhMucNhanSuPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.danhmuc_nhansu_feature(RolePositionEnum.ADMINISTRATOR))
            {
                danhMucNhanSuPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.DM_NS, m_menus), ApplicationRoleId = Administrator.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(danhMucNhanSuPermission);
        }
        if (Supplier != null)
        {
            var danhMucNhanSuPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.danhmuc_nhansu_feature(RolePositionEnum.EMPLOYEE))
            {
                danhMucNhanSuPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.DM_NS, m_menus), ApplicationRoleId = Supplier.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(danhMucNhanSuPermission);
        }
        if (Sale != null)
        {
            var danhMucNhanSuPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.danhmuc_nhansu_feature(RolePositionEnum.EMPLOYEE))
            {
                danhMucNhanSuPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.DM_NS, m_menus), ApplicationRoleId = Sale.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(danhMucNhanSuPermission);
        }
        #endregion
        #region CoHoi
        if (SaleDirector != null)
        {
            // Cơ hội
            var coHoiPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_cohoi_feature(RolePositionEnum.MANAGER))
            {
                coHoiPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_CH, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(coHoiPermission);
        }
        if (Sale != null)
        {
            // Cơ hội
            var coHoiPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_cohoi_feature(RolePositionEnum.EMPLOYEE))
            {
                coHoiPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_CH, m_menus), ApplicationRoleId = Sale.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(coHoiPermission);
        }
        if (Supplier != null)
        {
            // Cơ hội
            var coHoiPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_cohoi_feature(RolePositionEnum.EMPLOYEE))
            {
                coHoiPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_CH, m_menus), ApplicationRoleId = Supplier.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(coHoiPermission);
        }
        #endregion
        #region MucTieu
        if (SaleDirector != null)
        {
            var mucTieuPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_mt_feature(RolePositionEnum.MANAGER))
            {
                mucTieuPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_MT, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(mucTieuPermission);
        }
        if (Administrator != null)
        {
            var mucTieuPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_mt_feature(RolePositionEnum.ADMINISTRATOR))
            {
                mucTieuPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_MT, m_menus), ApplicationRoleId = Administrator.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(mucTieuPermission);
        }
        if (Supplier != null)
        {
            var mucTieuPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_mt_feature(RolePositionEnum.EMPLOYEE))
            {
                mucTieuPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_MT, m_menus), ApplicationRoleId = Supplier.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(mucTieuPermission);
        }
        if (Sale != null)
        {
            var mucTieuPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_mt_feature(RolePositionEnum.EMPLOYEE))
            {
                mucTieuPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_MT, m_menus), ApplicationRoleId = Sale.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(mucTieuPermission);
        }
        #endregion
        #region QuyenLoi
        if (SaleDirector != null)
        {
            var quyenLoiPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_ql_feature(RolePositionEnum.MANAGER))
            {
                quyenLoiPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_QL, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(quyenLoiPermission);
        }
        if (Administrator != null)
        {
            var quyenLoiPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_ql_feature(RolePositionEnum.ADMINISTRATOR))
            {
                quyenLoiPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_QL, m_menus), ApplicationRoleId = Administrator.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(quyenLoiPermission);
        }
        if (Sale != null)
        {
            var quyenLoiPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_ql_feature(RolePositionEnum.EMPLOYEE))
            {
                quyenLoiPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_QL, m_menus), ApplicationRoleId = Sale.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(quyenLoiPermission);
        }
        #endregion
        #region MoiQuanHe
        if (SaleDirector != null)
        {
            var moiQuanHePermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_mqh_feature(RolePositionEnum.MANAGER))
            {
                moiQuanHePermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_MQH, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(moiQuanHePermission);
        }
        if (Administrator != null)
        {
            var moiQuanHePermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_mqh_feature(RolePositionEnum.ADMINISTRATOR))
            {
                moiQuanHePermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_MQH, m_menus), ApplicationRoleId = Administrator.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(moiQuanHePermission);
        }
        if (Sale != null)
        {
            var moiQuanHePermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_mqh_feature(RolePositionEnum.EMPLOYEE))
            {
                moiQuanHePermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_MQH, m_menus), ApplicationRoleId = Sale.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(moiQuanHePermission);
        }
        if (Supplier != null)
        {
            var moiQuanHePermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_mqh_feature(RolePositionEnum.EMPLOYEE))
            {
                moiQuanHePermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_MQH, m_menus), ApplicationRoleId = Supplier.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(moiQuanHePermission);
        }
        #endregion
        #region SaleKit
        if (SaleDirector != null)
        {
            var saleKitPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_salekit_feature(RolePositionEnum.MANAGER))
            {
                saleKitPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_SK, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(saleKitPermission);
        }
        if (Administrator != null)
        {
            var saleKitPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_salekit_feature(RolePositionEnum.ADMINISTRATOR))
            {
                saleKitPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_SK, m_menus), ApplicationRoleId = Administrator.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(saleKitPermission);
        }
        if (Supplier != null)
        {
            var saleKitPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_salekit_feature(RolePositionEnum.EMPLOYEE))
            {
                saleKitPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_SK, m_menus), ApplicationRoleId = Supplier.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(saleKitPermission);
        }
        if (Sale != null)
        {
            var saleKitPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_salekit_feature(RolePositionEnum.EMPLOYEE))
            {
                saleKitPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_SK), ApplicationRoleId = Sale.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(saleKitPermission);
        }
        #endregion
        #region Elearning
        if (SaleDirector != null)
        {
            var elPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_elearning_feature(RolePositionEnum.MANAGER))
            {
                elPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_EL_GD, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(elPermission);
        }
        if (Administrator != null)
        {
            var elPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_elearning_feature(RolePositionEnum.ADMINISTRATOR))
            {
                elPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_EL_NCN, m_menus), ApplicationRoleId = Administrator.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(elPermission);
        }
        if (Supplier != null)
        {
            var elPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_elearning_feature(RolePositionEnum.EMPLOYEE))
            {
                elPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_EL_NV, m_menus), ApplicationRoleId = Supplier.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(elPermission);
        }
        if (Sale != null)
        {
            var elPermission = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.sale_elearning_feature(RolePositionEnum.EMPLOYEE))
            {
                elPermission.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_EL_NV, m_menus), ApplicationRoleId = Sale.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(elPermission);
        }
        #endregion
        #region PhanQuyen_SaleKit
        //if (SaleDirector != null)
        //{
        //	var phanQuyenSaleKitPermission = new List<ApplicationRoleDetail>();
        //	foreach (var fea in FeatureDataConstant.sale_phanquyen_salekit_feature())
        //	{
        //		phanQuyenSaleKitPermission.Add(
        //			new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_SK_PQ, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
        //		);
        //	}
        //	_context.ApplicationRoleDetails.AddRange(phanQuyenSaleKitPermission);
        //}
        //if (Administrator != null)
        //{
        //	var phanQuyenSaleKitPermission = new List<ApplicationRoleDetail>();
        //	foreach (var fea in FeatureDataConstant.sale_phanquyen_salekit_feature())
        //	{
        //		phanQuyenSaleKitPermission.Add(
        //			new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.Sale_SK_PQ, m_menus), ApplicationRoleId = Administrator.Id, FeatureId = fea }
        //		);
        //	}
        //	_context.ApplicationRoleDetails.AddRange(phanQuyenSaleKitPermission);
        //}
        #endregion
        #region NhanSu
        var lstNhanSuCode = new List<string>()
        {
            MenuType.NS_TTNS, // MenuType.NS_BCTN, MenuType.NS_TTTC,
		};
        var lstNhanSu = _context.Menus.Include(s => s.FeatureMenus).Where(s => s.Code != null && lstNhanSuCode.Contains(s.Code)).ToList();
        foreach (var ns in lstNhanSu)
        {
            if (SaleDirector != null)
            {
                var nhanSuPermission = new List<ApplicationRoleDetail>();
                foreach (var fea in FeatureDataConstant.nhanhsu_feature(RolePositionEnum.MANAGER))
                {
                    nhanSuPermission.Add(
                        new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = ns.Id, ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                    );
                }
                _context.ApplicationRoleDetails.AddRange(nhanSuPermission);
            }
            if (Administrator != null)
            {
                var nhanSuPermission = new List<ApplicationRoleDetail>();
                foreach (var fea in FeatureDataConstant.nhanhsu_feature(RolePositionEnum.ADMINISTRATOR))
                {
                    nhanSuPermission.Add(
                        new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = ns.Id, ApplicationRoleId = Administrator.Id, FeatureId = fea }
                    );
                }
                _context.ApplicationRoleDetails.AddRange(nhanSuPermission);
            }
            if (Sale != null)
            {
                var nhanSuPermission = new List<ApplicationRoleDetail>();
                foreach (var fea in FeatureDataConstant.nhanhsu_feature(RolePositionEnum.EMPLOYEE))
                {
                    nhanSuPermission.Add(
                        new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = ns.Id, ApplicationRoleId = Sale.Id, FeatureId = fea }
                    );
                }
                _context.ApplicationRoleDetails.AddRange(nhanSuPermission);
            }
            if (Supplier != null)
            {
                var nhanSuPermission = new List<ApplicationRoleDetail>();
                foreach (var fea in FeatureDataConstant.nhanhsu_feature(RolePositionEnum.EMPLOYEE))
                {
                    nhanSuPermission.Add(
                        new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = ns.Id, ApplicationRoleId = Supplier.Id, FeatureId = fea }
                    );
                }
                _context.ApplicationRoleDetails.AddRange(nhanSuPermission);
            }
        }
        // ****************** thông tin thu nhập **************
        if (SaleDirector != null)
        {
            var thuNhapFeature = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.thu_nhap_feature(RolePositionEnum.MANAGER))
            {
                thuNhapFeature.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.NS_TTTN, m_menus), ApplicationRoleId = SaleDirector.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(thuNhapFeature);
        }
        if (Administrator != null)
        {
            var thuNhapFeature = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.thu_nhap_feature(RolePositionEnum.ADMINISTRATOR))
            {
                thuNhapFeature.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.NS_TTTN, m_menus), ApplicationRoleId = Administrator.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(thuNhapFeature);
        }
        if (Supplier != null)
        {
            var thuNhapFeature = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.thu_nhap_feature(RolePositionEnum.EMPLOYEE))
            {
                thuNhapFeature.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.NS_TTTN, m_menus), ApplicationRoleId = Supplier.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(thuNhapFeature);
        }
        if (Sale != null)
        {
            var thuNhapFeature = new List<ApplicationRoleDetail>();
            foreach (var fea in FeatureDataConstant.thu_nhap_feature(RolePositionEnum.EMPLOYEE))
            {
                thuNhapFeature.Add(
                    new ApplicationRoleDetail() { Id = Guid.NewGuid(), MenuId = await getMenuId(MenuType.NS_TTTN, m_menus), ApplicationRoleId = Sale.Id, FeatureId = fea }
                );
            }
            _context.ApplicationRoleDetails.AddRange(thuNhapFeature);
        }

        #endregion

        var result = await _context.SaveChangesAsync();
        return result;
    }
}