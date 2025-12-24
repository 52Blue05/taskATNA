using Sale_Saas.Domain.Common;
namespace Sale_Saas.Application.Features.SupplierFeature.Dto
{
    public class SupplierDto : BaseEntityDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Review { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Supplier, SupplierDto>();
            }
        }
    }
}
