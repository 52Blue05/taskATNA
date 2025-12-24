namespace Sale_Saas.Application.Models.Benefit
{
    public class UpdateStatusBenefitRequest : UpdateStatusRequest
    {
        public decimal? MonthlySalary { get; set; }
        public decimal? TargetSalary { get; set; }
        public decimal? TotalSalary { get; set; }
        public decimal? SuggestMonthlySalary { get; set; }
        public decimal? SuggestTargetSalary { get; set; }
        public decimal? SuggestTotalSalary { get; set; }
    }
}
