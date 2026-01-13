namespace Sale_Saas.Application.Features.OpportunityFeature.Dto;

public class CreateOrUpdateOpportunityRequest
{
    public Guid? IdCoHoi { get; set; }
    public Guid? IdKhachHang { get; set; }
    //public string? TenKhachHang { get; set; }
    public Guid? IdNhanVien { get; set; } // optionals
    public string? AnhDaiDien { get; set; }
    public string? NhuCau { get; set; }
    public string? NguoiQuyetDinh { get; set; }
    public string? NguoiPhuTrachKiThuat { get; set; }
    public string? NguoiThuHuong { get; set; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }
    public string? TongTien { get; set; }
    public string? KhaNangThang { get; set; }
    public string? LoaiTien { get; set; }
    public string? QuyDoi { get; set; }
    public string? VonDuKien { get; set; }
    public string? TienHoaHong { get; set; }
    //public Guid? IdTrangThai { get; set; }
    public List<CreateOrUpdateOpponentRequest>? DanhSachDoiThu
    { get; set; } = new List<CreateOrUpdateOpponentRequest>();
    public string? ChienLuoc { get; set; }
    public List<CreateOrUpdateOpportunityHistoryRequest>? DanhSachLichSuTuongTac { get; set; } = new List<CreateOrUpdateOpportunityHistoryRequest>();
}

public class CreateOrUpdateOpponentRequest
{
    public Guid? Id { get; set; }
    public string? TenDoiThu { get; set; }
    public string? DiemManh { get; set; }
    public string? DiemYeu { get; set; }
}

public class CreateOrUpdateOpportunityHistoryRequest
{
    public Guid? Id { get; set; }
    public string? MucTieu { get; set; }
    public string? HoatDong { get; set; }
    public DateTime? ThoiDiem { get; set; }
    public string? KetQua { get; set; }
}
