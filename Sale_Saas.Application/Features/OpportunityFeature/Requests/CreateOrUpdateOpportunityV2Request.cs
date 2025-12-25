namespace Sale_Saas.Application.Features.OpportunityFeature.Requests;

public class CreateOrUpdateOpportunityV2Request
{
    public Guid? Id { get; set; }
    public Guid? CustomerId { get; set; }                // Customer
    public string? Accountable { get; set; }       // Người quyết định
    public string? TechnicalLead { get; set; }     // Người phụ trách kĩ thuật
    public string? Beneficiary { get; set; }       // Người thụ hưởng
    public string? Need { get; set; }              // Nhu cầu
    public DateTime? OpportunityStartDate { get; set; }  // Ngày bắt đầu cơ hội
    public DateTime? OpportunityEndDate { get; set; }    // Ngày kết thúc cơ hội
    public decimal? Budget { get; set; }           // Ngân sách 
    public string? TypeMoney { get; set; }         // Loại tiền
    public decimal? CurrencyConversion { get; set; }     // Quy đổi tiền tệ
    public decimal? EstimatedMoney { get; set; }   // Vốn dự kiện
    public decimal? CommissionMoney { get; set; }  // Tiền hoa hồng
    public string? Strategy { get; set; }                // Chiến lượng AT&A
    public string? WinningOppotunity { get; set; }       // Tỉ lệ chiến thắng
    public string? Reason { get; set; }
    public DateTime? EstimatedTime { get; set; } // Thời gian dự kiến
    public Guid? CreatedApplicationUserId { get; set; }
    public Guid? LastModifiedApplicationUserId { get; set; }
    public Guid? ApplicationRoleId { get; set; }
    public List<CreateOrUpdateOpportunityOpponentV2Request> OpportunityOpponents { get; set; }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<OpportunityOpponent, CreateOrUpdateOpportunityOpponentV2Request>().ReverseMap();
            CreateMap<Opportunity, CreateOrUpdateOpportunityV2Request>().ReverseMap();
        }
    }
}

public class CreateOrUpdateOpportunityOpponentV2Request
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Strength { get; set; }
    public string? Weakness { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<OpportunityOpponent, CreateOrUpdateOpportunityOpponentV2Request>().ReverseMap();
        }
    }
}
