using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Infrastructure.Services;
using System.Reflection;

namespace Sale_Saas.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    public string CurrentTenantId { get; set; }
    public string CurrentTenantConnectionString { get; set; }
    public ApplicationDbContext(ICurrentTenantService currentTenantService, DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        _currentTenantService = currentTenantService;
        CurrentTenantId = _currentTenantService.TenantId;
        CurrentTenantConnectionString = _currentTenantService.ConnectionString;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        string tenantConnectionString = CurrentTenantConnectionString;
        if (!string.IsNullOrEmpty(tenantConnectionString)) // use tenant db if one is specified
        {
            _ = optionsBuilder.UseNpgsql(tenantConnectionString);
        }
    }
    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
    public DbSet<ApplicationRoleDetail> ApplicationRoleDetails => Set<ApplicationRoleDetail>();
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<ApplicationUserStatus> ApplicationUserStatuses => Set<ApplicationUserStatus>();
    public DbSet<MailServerInfo> MailServerInfos => Set<MailServerInfo>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<GainsQuestion> GainsQuestions => Set<GainsQuestion>();
    public DbSet<RelationshipLevel> RelationshipLevels => Set<RelationshipLevel>();
    public DbSet<Relationship> Relationships => Set<Relationship>();
    public DbSet<Gains> Gains => Set<Gains>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<EventLog> EventLogs => Set<EventLog>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<GoalStatus> GoalStatuses => Set<GoalStatus>();
    public DbSet<Relationship_GainsQuestion> Relationship_GainsQuestions => Set<Relationship_GainsQuestion>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractStatus> ContractStatuses => Set<ContractStatus>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectStatus> ProjectStatuses => Set<ProjectStatus>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<OpportunityStatus> OpportunityStatuses => Set<OpportunityStatus>();
    public DbSet<OpportunityHistory> OpportunityHistories => Set<OpportunityHistory>();
    public DbSet<Benefit> Benefits => Set<Benefit>();
    public DbSet<BenefitStatus> BenefitStatuses => Set<BenefitStatus>();
    public DbSet<BenefitHistory> BenefitHistories => Set<BenefitHistory>();
    public DbSet<RelationshipStatus> RelationshipStatuses => Set<RelationshipStatus>();
    public DbSet<EmployeeSalary> EmployeeSalarys => Set<EmployeeSalary>();
    public DbSet<SaleKit> SaleKits => Set<SaleKit>();
    public DbSet<ApplicationRoleSaleKit> ApplicationRole_SaleKits => Set<ApplicationRoleSaleKit>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<EmployeeSalaryDetail> EmployeeSalaryDetails => Set<EmployeeSalaryDetail>();
    public DbSet<RolePosition> RolePositions => Set<RolePosition>();
    public DbSet<Media> Medias => Set<Media>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<FeatureMenu> FeatureMenus => Set<FeatureMenu>();
    public DbSet<RolePositionFeatureMenu> RolePositionFeatureMenus => Set<RolePositionFeatureMenu>();

    public DbSet<Syllabus> Syllabus => Set<Syllabus>();
    public DbSet<ApplicationUserSyllabus> ApplicationUserSyllabus => Set<ApplicationUserSyllabus>();
    public DbSet<Units> Units => Set<Units>();
    public DbSet<SyllabusUnits> SyllabusUnits => Set<SyllabusUnits>();
    public DbSet<LoveUnits> LoveUnits => Set<LoveUnits>();
    public DbSet<Lessions> Lessions => Set<Lessions>();
    public DbSet<UnitQuestions> UnitQuestions => Set<UnitQuestions>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<OpportunityOpponent> OpportunityOpponents => Set<OpportunityOpponent>();
    public DbSet<UserLessonProgress> UserLessonProgresses => Set<UserLessonProgress>();
    public DbSet<UnitDependency> UnitDependencies => Set<UnitDependency>();
    public DbSet<SyllabusDependency> SyllabusDependencies => Set<SyllabusDependency>();

    public DbSet<RelationshipHistory> RelationshipHistories => Set<RelationshipHistory>();
    public DbSet<GainsSchool> GainsSchools => Set<GainsSchool>();
    public DbSet<GainsFamily> GainsFamilies => Set<GainsFamily>();
    public DbSet<RelationshipCustomer> RelationshipCustomers => Set<RelationshipCustomer>();
    public DbSet<TargetFluctuation> TargetFluctuations => Set<TargetFluctuation>();
    public DbSet<Criteria> Criterias => Set<Criteria>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    // On Save Changes - write tenant Id to table
    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                    entry.Entity.TenantId = CurrentTenantId;
                    break;
            }
        }
        var result = base.SaveChanges();
        return result;
    }
    public void SetConnectString(string connectString)
    {
        base.Database.SetConnectionString(connectString);
    }

    public string GetConnectString()
    {
        return base.Database.GetConnectionString();
    }

    public void ClearChangeTracker()
    {
        base.ChangeTracker.Clear();
    }

}