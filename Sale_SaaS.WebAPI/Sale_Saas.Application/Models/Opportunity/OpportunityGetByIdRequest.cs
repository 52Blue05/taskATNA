namespace Sale_Saas.Application.Models.Opportunity
{
    public class OpportunityGetByIdRequest : GetListWithPaginationQueryRequest
    {
        public Guid? OpportunityId { get; set; }
    }
}
