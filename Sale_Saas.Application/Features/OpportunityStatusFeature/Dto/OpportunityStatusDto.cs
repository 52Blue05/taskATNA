namespace Sale_Saas.Application.Features.OpportunityStatusFeature.Dto
{
    public class OpportunityStatusDto : BaseEntityDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<OpportunityStatus, OpportunityStatusDto>();
            }
        }
    }
}
