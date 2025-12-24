using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RelationshipFeature.RequestModels
{
     public class UpdateLevelRequest
     {
          public Guid Id { set; get; }
          public Guid ApplicationUserId { get; set; }
          public string? Level { get; set; }
          public string? Locale { get; set; } = LocaleEnum.vi_VN.ToString();
     }
}
