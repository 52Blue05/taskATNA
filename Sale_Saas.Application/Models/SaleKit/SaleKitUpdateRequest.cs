using Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Dto;

namespace Sale_Saas.Application.Models.SaleKit
{
    public class SaleKitUpdateRequest
    {
        public Guid? Id { set; get; }
        public List<ApplicationRoleSaleKitDto>? Data { set; get; }
        public Guid ApplicationRoleId { get; set; }
        public Guid? CreatedApplicationUserId { set; get; }
        public Guid? LastModifiedApplicationUserId { set; get; }
    }
}
