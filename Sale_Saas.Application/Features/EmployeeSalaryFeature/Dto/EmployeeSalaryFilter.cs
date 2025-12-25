namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto
{
    public class EmployeeSalaryFilter : BaseEntityDto
    {
        public string? EmployeeCode { get; set; }
        public Guid? UserId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public Guid? RoleId { get; set; }
    }

    public class EmployeeSalarySumFilter 
    {
        public string? EmployeeCode { get; set; }
        public Guid? UserId { get; set; }
        public string? ConnectString { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }

    }
}
