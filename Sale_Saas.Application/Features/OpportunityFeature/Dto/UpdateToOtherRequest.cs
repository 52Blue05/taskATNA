namespace Sale_Saas.Application.Features.OpportunityFeature.Dto;

public class UpdateToOtherRequest
{
    public Guid? IdCoHoi { get; set; }
    public Guid? IdNhanVien { get; set; }
    public Guid? IdTrangThai { get; set; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }
    public string? LyDo { get; set; }
}
