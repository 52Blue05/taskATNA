namespace Sale_Saas.Application.Models.Opportunity
{
    public class UpdateStatusOpportunityRequest : UpdateStatusRequest
    {
        public string? Reason { get; set; }
    }
}
