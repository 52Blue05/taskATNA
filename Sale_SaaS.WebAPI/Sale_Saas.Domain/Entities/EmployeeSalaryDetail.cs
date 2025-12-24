using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities
{
    public class EmployeeSalaryDetail : BaseAuditableEntity
    {
        public string? EmployeeCode { get; set; }
        public Guid? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public decimal? IncomeEta { get; set; }
        public decimal? IncomeReal { get; set; }
        [MaxLength(50)]
        public string? UserName { get; set; }
        [MaxLength(255)]
        public string? TypeCP { get; set; }
        [MaxLength(255)]
        public string? ProjectName { get; set; }
        public DateTime? TimeSpent { get; set; }   
        public string? Note { get; set; }
        public string? Content { get; set; }
    }

    public class EmployeeSalaryDetailTenant
    {
        public string ConnecttionStr { get; set; }
        public EmployeeSalaryDetail EmployeeSalaryDetail { get; set; }
    }

    public class EmployeeSalaryDetailImport
    {
       public Guid userId {  get; set; }
       public IFormFile file { get; set; }
    }
}
