namespace Sale_Saas.Domain.Entities
{
    public class Opportunity : BaseAuditableEntity
    {
        public string? CustomerName { get; set; }      // Khách hàng
        public string? Need { get; set; }              // Nhu cầu
        public string? Accountable { get; set; }       // Người quyết định
        public string? TechnicalLead { get; set; }     // Người phụ trách kĩ thuật
        public string? Beneficiary { get; set; }       // Người thụ hưởng
        public Guid? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public DateTime? EstimatedTime { get; set; }
        public string? Reason { get; set; }                  // Lí do
        public decimal? Budget { get; set; }                 // Ngân sách 
        public decimal? EstimatedMoney { get; set; }         // Vốn dự kiện
        public decimal? CommissionMoney { get; set; }        // Tiền hoa hồng
        public string? Opponent1 { get; set; }               // Đối thủ 1 
        public string? Opponent1Attribute { get; set; }      // Điểm mạnh điểm yếu Đối thủ 1 
        public string? Opponent1Strength { get; set; }       // Điểm mạnh của Đối thủ 1
        public string? Opponent1Weakness { get; set; }       // Điểm yếu của Đối thủ 1
        public string? Opponent2 { get; set; }               // Đối thủ 2
        public string? Opponent2Attribute { get; set; }      // Điểm mạnh điểm yếu Đối thủ 2
        public string? Opponent2Strength { get; set; }       // Điểm mạnh của Đối thủ 2
        public string? Opponent2Weakness { get; set; }       // Điểm yếu của Đối thủ 2
        public string? Opponent3 { get; set; }               // Đối thủ 2
        public string? Opponent3Attribute { get; set; }      // Điểm mạnh điểm yếu Đối thủ 2
        public string? Opponent3Strength { get; set; }       // Điểm mạnh của Đối thủ 2
        public string? Opponent3Weakness { get; set; }       // Điểm yếu của Đối thủ 2
        public string? Strategy { get; set; }                // Chiến lượng AT&A
        public DateTime? LastTimeInteract { get; set; }      // Lần cuối cương tác
        public string? WinningOppotunity { get; set; }       // Tỉ lệ chiến thắng
        public DateTime? OpportunityStartDate { get; set; }  // Ngày bắt đầu cơ hội
        public DateTime? OpportunityEndDate { get; set; }    // Ngày kết thúc cơ hội
        public string? TypeMoney { get; set; }               // Loại tiền
        public decimal? TotalMoney { get; set; }             // Tổng tiền
        public string? ReasonClosed { get; set; }            // Lý do đóng cơ hội
        public decimal? CurrencyConversion { get; set; }     // Quy đổi tiền tệ
        public Guid? CustomerId { get; set; }                // Customer
        public Customer? Customer { get; set; }
        public Guid? ApplicationRoleId { get; set; }
        public ApplicationRole? ApplicationRole { get; set; }
        public Guid? OpportunityStatusId { get; set; }
        public OpportunityStatus? OpportunityStatus { get; set; }
        public ICollection<OpportunityHistory>? OpportunityHistories { set; get; }
        public ICollection<OpportunityOpponent>? OpportunityOpponents { set; get; }
    }
}
