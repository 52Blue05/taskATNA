namespace Sale_Saas.Application.Models.SaleKit
{
	public class SaleKitGetListWithPaginationQueryRequest : GetListWithPaginationQueryRequest
	{
		public Guid? ParentId { get; set; } = null;
	}
}
