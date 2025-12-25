namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Dto
{
    public class RelationshipLevelDto : BaseEntityDto
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public string Review { get; set; } = "";
        public int PointFrom { get; set; }
        public int PointTo { get; set; }
        public int SortOrder { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<RelationshipLevel, RelationshipLevelDto>();
            }
        }
    }
}
