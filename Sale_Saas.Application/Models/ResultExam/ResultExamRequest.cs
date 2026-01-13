namespace Sale_Saas.Application.Models.ResultExam;

public class ResultExamRequest
{
    public Guid ApplicationUserId { get; set; }
    public Guid UnitQuestionId { get; set; }
    public List<Guid> submittedAnswerIds { get; set; }
    public int? TimeCompletion { get; set; }
}
