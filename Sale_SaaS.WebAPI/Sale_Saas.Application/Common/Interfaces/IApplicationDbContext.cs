namespace Sale_Saas.Application.Common.Interfaces;
public interface IApplicationDbContext
{
    DbSet<ApplicationRole> ApplicationRoles { get; }
    DbSet<ApplicationRoleDetail> ApplicationRoleDetails { get; }
    DbSet<ApplicationUser> ApplicationUsers { get; }
    DbSet<ApplicationUserStatus> ApplicationUserStatuses { get; }
    DbSet<MailServerInfo> MailServerInfos { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Service> Services { get; }
    DbSet<GainsQuestion> GainsQuestions { get; }
    DbSet<RelationshipLevel> RelationshipLevels { get; }
    DbSet<Relationship> Relationships { get; }
    DbSet<Gains> Gains { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Menu> Menus { get; }
    DbSet<Goal> Goals { get; }
    DbSet<GoalStatus> GoalStatuses { get; }
    DbSet<EventLog> EventLogs { get; }
    DbSet<Relationship_GainsQuestion> Relationship_GainsQuestions { get; }
    DbSet<Contract> Contracts { get; }
    DbSet<ContractStatus> ContractStatuses { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectStatus> ProjectStatuses { get; }
    DbSet<Opportunity> Opportunities { get; }
    DbSet<OpportunityStatus> OpportunityStatuses { get; }
    DbSet<OpportunityHistory> OpportunityHistories { get; }
    DbSet<Benefit> Benefits { get; }
    DbSet<BenefitHistory> BenefitHistories { get; }
    DbSet<BenefitStatus> BenefitStatuses { get; }
    DbSet<RelationshipStatus> RelationshipStatuses { get; }
    DbSet<EmployeeSalary> EmployeeSalarys { get; }
    DbSet<SaleKit> SaleKits { get; }
    DbSet<ApplicationRoleSaleKit> ApplicationRole_SaleKits { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<EmployeeSalaryDetail> EmployeeSalaryDetails { get; }
    DbSet<RolePosition> RolePositions { get; }
    DbSet<Media> Medias { get; }
    DbSet<Feature> Features { get; }
    DbSet<FeatureMenu> FeatureMenus { get; }
    DbSet<RolePositionFeatureMenu> RolePositionFeatureMenus { get; }
    DbSet<Syllabus> Syllabus { get; }
    DbSet<ApplicationUserSyllabus> ApplicationUserSyllabus { get; }
    DbSet<Units> Units { get; }
    DbSet<SyllabusUnits> SyllabusUnits { get; }
    DbSet<LoveUnits> LoveUnits { get; }
    DbSet<Lessions> Lessions { get; }
    DbSet<UnitQuestions> UnitQuestions { get; }
    DbSet<Question> Questions { get; }
    DbSet<Answer> Answers { get; }
    DbSet<Result> Results { get; }
    DbSet<OpportunityOpponent> OpportunityOpponents { get; }
    DbSet<UserLessonProgress> UserLessonProgresses { get; }
    DbSet<UnitDependency> UnitDependencies { get; }
    DbSet<SyllabusDependency> SyllabusDependencies { get; }

    DbSet<RelationshipHistory> RelationshipHistories { get; }
    DbSet<GainsSchool> GainsSchools { get; }
    DbSet<GainsFamily> GainsFamilies { get; }
    DbSet<RelationshipCustomer> RelationshipCustomers { get; }
    DbSet<TargetFluctuation> TargetFluctuations { get; }
    DbSet<Criteria> Criterias { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    void SetConnectString(string connectString);
    void ClearChangeTracker();
    void Dispose();
    ValueTask DisposeAsync();
}
