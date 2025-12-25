using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities
{
    public class EmployeeSalary : BaseAuditableEntity
    {
        [MaxLength(50)]
        public string EmployeeCode { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public decimal? IncomeBeforeTax { get; set; }
        public decimal? IncomeNonTax { get; set; }
        public decimal? Dependent { get; set; }
        public decimal? Insurance { get; set; }
        public decimal? IncomeTax { get; set; }
        public decimal? PersonalIncomeTax { get; set; }
        public decimal? IncomeRecevied { get; set; }
        public Guid? RoleId { get; set; }
        public ApplicationRole? Role { get; set; }
        public string? Note { get; set; }
        public decimal? IncomeOther { get; set; }
        public decimal? IncomeByRole { get; set; }
        public decimal? MyDependent { get; set; }   // GTGC BanThan
        public int? NumberDependent { get; set; }   // SoNguoiPhuThuoc
        public decimal? UnitDependent { get; set; } // GTGC NguoiPhuThuoc => Dependent = MyDependent + (NumberDependent * UnitDependent)
        public decimal? AmountInsurance { get; set; } // MucDongBaoHiem
        public decimal? PercentInsurance { get; set; } // TyLeDongBaoHiem => Insurace = MucDongBaoHiem * TyLeDongBaoHiem
    }

    public class EmployeeTenant
    {
        public string ConnecttionStr { get; set; }
        public EmployeeSalary EmployeeSalary { get; set; }
    }
}
