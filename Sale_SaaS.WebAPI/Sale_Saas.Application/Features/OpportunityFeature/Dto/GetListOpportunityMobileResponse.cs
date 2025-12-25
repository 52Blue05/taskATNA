using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.OpportunityFeature.Dto;

public class GetListOpportunityMobileResponse
{
    public int? SoCoHoi { get; set; }
    public string? TongTienCoHoi { get; set; }
    public string? LoaiTien { get; set; }
    public int? SoCoHoiDaDong { get; set; }
    public string? TongTienCoHoiDaDong { get; set; }
    public List<GetOpportunityMobileDto> DanhSachCoHoi { get; set; } = new List<GetOpportunityMobileDto>();
}

public class GetOpportunityMobileDto
{
    public Guid IdCoHoi { get; set; }
    public Guid IdNhanVien { get; set; }
    public string? TenNhanVien { get; set; }
    public string? TrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public string? NhuCau { get; set; }
    public string? MaKhachHang { get; set; }
    public string? TenKhachHang { get; set; }
    public string? NguoiQuyetDinh { get; set; }
    public string? NguoiPhuTrachKiThuat { get; set; }
    public string? NguoiThuHuong { get; set; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }
    public decimal? TongTien { get; set; }
    public string? KhaNangThang { get; set; }
    public string? LoaiTien { get; set; }
    public string? LyDo { get; set; }
    public decimal? QuyDoi { get; set; }
    public string? VonDuKien { get; set; }
    public string? TienHoaHong { get; set; }

    public UserBasicInfoDto NguoiPhuTrach { get; set; } = new UserBasicInfoDto();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Opportunity, GetOpportunityMobileDto>()
                .ForMember(dest => dest.NguoiPhuTrach, opt => opt.MapFrom(src => src.ApplicationUser));
        }
    }
}
