namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class IncomeNonTaxDetailByYearDto
{
    public string? nam { get; set; }
    public string? cacKhoanThuNhapKhongChiuThue { get; set; }
    public List<IncomeNonTaxDetailByMonthDto> chiTietTheoThang { get; set; }
}

public class IncomeNonTaxDetailByMonthDto
{
    public string? thang { get; set; }
    public string? thuNhap { get; set; }
}