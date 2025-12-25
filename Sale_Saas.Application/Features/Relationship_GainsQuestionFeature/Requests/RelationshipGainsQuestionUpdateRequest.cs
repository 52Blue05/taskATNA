namespace Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Requests;

public class RelationshipGainsQuestionUpdateRequest
{
    public Guid GainsQuestionId { get; set; }
    public Guid RelationshipId { get; set; }
    public bool Answer { get; set; }
    public string? AnswerDetail { get; set; }
}
