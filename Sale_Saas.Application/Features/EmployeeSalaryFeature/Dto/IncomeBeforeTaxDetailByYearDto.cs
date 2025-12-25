namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class IncomeBeforeTaxDetailByYearDto
{
    public string? nam { get; set; }
    public string? tongThuNhapTruocThue { get; set; }
    public List<IncomeBeforeTaxDetailByMonthDto> chiTietTheoThang { get; set; }
}

public class IncomeBeforeTaxDetailByMonthDto
{
    public string? thang { get; set; }
    public string? thuNhap { get; set; }
    public string? cacKhoanThuNhapKhac { get; set; }
    public List<IncomeOfRoleMobileByMonthDto> chiTietVaiTro { get; set; }
}

public class RoleOfUserMobileDto
{
    public Guid? Id { get; set; }
    public string? RoleName { get; set; }
}

public class IncomeOfRoleMobileByMonthDto
{
    public string? TenVaiTro { get; set; }
    public string? ThuNhapTheoVaiTro { get; set; }
}

public class IncomeOtherByMonth
{
    public int? Month { get; set; }
    public decimal? IncomeOther { get; set; }
}
