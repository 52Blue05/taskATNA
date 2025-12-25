namespace Sale_Saas.Application.Features.GainsFeature.RequestModels
{
     public class Gains_AddOrUpdateRequest
     {
          public Guid? Id { set; get; }
          public Dictionary<string, object>? Data { set; get; }
          public Guid? CreatedApplicationUserId { set; get; }
          public Guid? LastModifiedApplicationUserId { set; get; }
          public List<CustomerByGoal>? Customers { get; set; }
     }
}
