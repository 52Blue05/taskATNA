namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class IncomeReceivedDetailByYearDto
{
    public string? nam { get; set; }
    public string? tongThuNhapNhanDuoc { get; set; }
    public List<IncomeReceivedDetailByMonthDto> chiTietTheoThang { get; set; }
}

public class IncomeReceivedDetailByMonthDto
{
    public string? thang { get; set; }
    public string? thuNhap { get; set; }
}
