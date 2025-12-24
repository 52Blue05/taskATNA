namespace Sale_Saas.Application.Models.Role_SaleKit
{
    public class RoleSaleKitGetListWithPaginationQueryRequest : GetListWithPaginationQueryRequest
    {
		public Guid? ParentId { get; set; } = null;
	}
}
