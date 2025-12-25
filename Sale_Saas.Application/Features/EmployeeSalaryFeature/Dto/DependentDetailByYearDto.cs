namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class DependentDetailByYearDto
{
    public string? nam { get; set; }
    public string? tongGiamTruGiaCanh { get; set; }
    public List<DependentDetailByMonthDto> chiTietTheoThang { get; set; }
}

public class DependentDetailByMonthDto
{
    public string? thang { get; set; }
    public string? tong { get; set; }
    public string? gtgcBanThan { get; set; }
    public string? soNguoiPhuThuoc { get; set; }
    public string? gtgcNguoiPhuThuoc { get; set; }
}