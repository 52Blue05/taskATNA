using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Application.Features.GoalStatusFeature.Dto
{
    public class GoalStatusDto : BaseEntityDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<GoalStatus, GoalStatusDto>();
            }
        }
    }
}
