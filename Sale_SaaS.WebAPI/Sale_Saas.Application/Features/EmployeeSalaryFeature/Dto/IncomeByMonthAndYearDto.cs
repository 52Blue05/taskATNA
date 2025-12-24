namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class IncomeByMonthAndYearDto
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public decimal? IncomeBeforeTax { get; set; }
    public decimal? IncomeNonTax { get; set; }
    public decimal? Dependent { get; set; }
    public decimal? Insurance { get; set; }
    public decimal? IncomeTax { get; set; }
    public decimal? PersonalIncomeTax { get; set; }
    public decimal? IncomeReceived { get; set; }
    public decimal? MyDependent { get; set; }
    public int? NumberDependent { get; set; }
    public decimal? UnitDependent { get; set; }
    public decimal? AmountInsurance { get; set; }
    public decimal? PercentInsurance { get; set; }
    public DateTime? CreatedDate { get; set; }
}
