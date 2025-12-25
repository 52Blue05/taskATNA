namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto
{
    public class EmployeeSalaryDetailFilter : BaseEntityDto
    {
        public string? EmployeeCode { get; set; }
        public Guid? UserId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
