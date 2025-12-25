namespace Sale_Saas.Application.Models.EmployeeSalary
{
    public class EmployeeSalaryGetListWithPaginationRequest : GetListWithPaginationQueryRequest
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public List<Guid>? Ids { get; set; }
    }
}
