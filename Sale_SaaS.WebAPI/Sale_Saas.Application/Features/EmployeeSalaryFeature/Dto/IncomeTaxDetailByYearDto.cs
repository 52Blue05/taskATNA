namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class IncomeTaxDetailByYearDto
{
    public string? nam { get; set; }
    public string? tongThuNhapChiuThue { get; set; }
    public List<IncomeTaxDetailByMonthDto> chiTietTheoThang { get; set; }
}

public class IncomeTaxDetailByMonthDto
{
    public string? thang { get; set; }
    public string? thuNhap { get; set; }
}
