namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Requests;

public class EmployeeSalaryDetailRequest
{
    public IFormFile File { get; set; }
    public Guid UserId { get; set; }
}
