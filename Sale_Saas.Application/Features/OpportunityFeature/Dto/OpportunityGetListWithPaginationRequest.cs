namespace Sale_Saas.Application.Features.OpportunityFeature.Dto;

public class OpportunityGetListWithPaginationRequest : GetListWithPaginationQueryRequest
{
    public int? Year { get; set; }
}
