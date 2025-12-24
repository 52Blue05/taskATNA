using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class InsuranceDetailByYearDto
{
    public string? nam { get; set; }
    public string? tongBHXHNLD { get; set; }
    public List<InsuranceDetailByMonthDto> chiTietTheoThang { get; set; }
}

public class InsuranceDetailByMonthDto
{
    public string? thang { get; set; }
    public string? tong { get; set; }
    public string? mucDongBHXH { get; set; }
    public string? tyLeDong { get; set; }
}
