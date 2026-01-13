namespace Sale_Saas.Application.Features.RelationshipStatusFeature.Dto
{
    public class RelationshipStatusDto : BaseEntityDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<RelationshipStatus, RelationshipStatusDto>();
            }
        }
    }
}
