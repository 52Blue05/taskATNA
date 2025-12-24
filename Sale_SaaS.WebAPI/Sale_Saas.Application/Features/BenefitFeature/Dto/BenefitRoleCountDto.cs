namespace Sale_Saas.Application.Features.BenefitFeature.Dto
{
     public class BenefitRoleCountDto
     {
          public Guid? ApplicationUserId { get; set; }
          public int? QuantityOfRole { get; set; }
          public string? RolePositionId { get; set; }
        public Guid? ApplicationRoleId { get; set; }
    }
}
