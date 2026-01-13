namespace Sale_Saas.Application.Features.OpportunityFeature.Dto;

public class GetOpportunityDetailMobileResponse
{
    public Guid IdCoHoi { get; set; }
    public Guid? IdKhachHang { get; set; }
    public string? MaKhachHang { get; set; }
    public string? TenKhachHang { get; set; }
    //public string? AnhDaiDien { get; set; }
    public Guid? IdNhanVien { get; set; }
    public string? TenNhanVien { get; set; }
    public string? NhuCau { get; set; }
    public string? NguoiQuyetDinh { get; set; }
    public string? NguoiThuHuong { get; set; }
    public string? NguoiPhuTrachKiThuat { get; set; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }
    public string? TongTien { get; set; }
    public string? KhaNangThang { get; set; }
    public string? LoaiTien { get; set; }
    public string? QuyDoi { get; set; }
    public string? LyDo { get; set; }
    public string? VonDuKien { get; set; }
    public string? TienHoaHong { get; set; }
    //public Guid? IdTrangThai { get; set; }
    //public string? TrangThai { get; set; }
    public List<GetOpportunityOpponentDetailResponse>? DanhSachDoiThu { get; set; } = new List<GetOpportunityOpponentDetailResponse>();
    public string? ChienLuoc { get; set; }
    public List<GetOpportunityHistoryDetailResponse>? DanhSachLichSuTuongTac { get; set; } = new List<GetOpportunityHistoryDetailResponse>();
    public TrangThaiResponse TrangThai { get; set; }
}

public class GetOpportunityOpponentDetailResponse
{
    public Guid Id { get; set; }
    public string? TenDoiThu { get; set; }
    public string? DiemManh { get; set; }
    public string? DiemYeu { get; set; }
}

public class GetOpportunityHistoryDetailResponse
{
    public Guid Id { get; set; }
    public string? MucTieu { get; set; }
    public string? HoatDong { get; set; }
    public DateTime? ThoiDiem { get; set; }
    public string? KetQua { get; set; }
}

public class TrangThaiResponse
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}

