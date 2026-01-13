namespace Sale_Saas.Application.Features.TargetFluctuationFeature.Requests;

public class TargetFluctuationRequest
{
    public int? TargetYear { get; set; }
    public string? TypeMoney { get; set; }
    public decimal TargetSalary { get; set; }
    public decimal CompletionPercent { get; set; }
}
