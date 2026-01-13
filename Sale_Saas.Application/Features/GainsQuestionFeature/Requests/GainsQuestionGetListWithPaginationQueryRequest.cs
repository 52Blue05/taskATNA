namespace Sale_Saas.Application.Features.GainsQuestionFeature.Requests;

public class GainsQuestionGetListWithPaginationQueryRequest : GetListWithPaginationQueryRequest
{
    public Guid RelationshipId { get; set; }
}
