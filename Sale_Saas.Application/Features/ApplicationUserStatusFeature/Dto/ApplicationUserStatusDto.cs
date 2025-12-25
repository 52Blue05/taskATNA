

namespace Sale_Saas.Application.Features.ApplicationUserStatusFeature.Dto
{
    public class ApplicationUserStatusDto : BaseEntityDto
    {
        public string? Code { set; get; }
        public string? Name { set; get; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ApplicationUserStatus, ApplicationUserStatusDto>();
            }
        }
    }
}
