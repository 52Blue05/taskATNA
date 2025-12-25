namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class IncomePersonalTaxDetailByYearDto
{
    public string? nam { get; set; }
    public string? tongThueTNCNTamThu { get; set; }
    public List<IncomePersonalTaxDetailByMonthDto> chiTietTheoThang { get; set; }
}

public class IncomePersonalTaxDetailByMonthDto
{
    public string? thang { get; set; }
    public string? thueTNCN { get; set; }
}
