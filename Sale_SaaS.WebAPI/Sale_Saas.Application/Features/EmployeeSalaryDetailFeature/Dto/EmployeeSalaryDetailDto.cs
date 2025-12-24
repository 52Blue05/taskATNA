using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Dto
{
    public class EmployeeSalaryDetailDto : BaseEntityDto
    {
        public string? EmployeeCode { get; set; }
        public Guid? UserId { get; set; }
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

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<EmployeeSalaryDetail, EmployeeSalaryDetailDto>();
            }
        }
    }
}
