namespace Sale_Saas.Application.Features.OpportunityStatusFeature;

public static class Requests
{
    public class OpportunityStatusGetAllQueryRequest : GetAllQueryRequest
    {
        public string? StatusCode { get; set; }
    }
}
