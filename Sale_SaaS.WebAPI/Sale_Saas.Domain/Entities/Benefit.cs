namespace Sale_Saas.Domain.Entities
{
     public class Benefit : BaseAuditableEntity
     {
          public Guid? ApplicationUserId { get; set; }
          public ApplicationUser? ApplicationUser { get; set; }
          public decimal? TotalBenefit { get; set; }         // Tổng quyền lợi hiện tại
          public decimal? MonthlySalary { get; set; }       // Mức lương cố định hằng tháng
          public decimal? TargetSalary { get; set; }         // Tổng mức lương mục tiêu
          public decimal? TotalSalary { get; set; }          // Tổng mức lương biến động mục tiêu thực tế
          public decimal? SuggestMonthlySalary { get; set; }        // Mức lương cố định hằng tháng muốn chỉnh sửa
          public decimal? SuggestTargetSalary { get; set; }         // Tổng mức lương mục tiêu muốn chỉnh sửa
          public decimal? SuggestTotalSalary { get; set; }          // Tổng mức lương biến động muốn chỉnh sửa
          public decimal? EstimateBenefit { get; set; }            // Quyền lợi tạm tính
          public string? RolePositionId { get; set; }
          public Guid? ApplicationRoleId { get; set; }
          public ApplicationRole? ApplicationRole { get; set; }
          public Guid? BenefitStatusId { get; set; }
          public BenefitStatus? BenefitStatus { get; set; }
          public ICollection<BenefitHistory>? BenefitHistories { set; get; }
          public ICollection<TargetFluctuation>? TargetFluctuations { set; get; }
     }
}
