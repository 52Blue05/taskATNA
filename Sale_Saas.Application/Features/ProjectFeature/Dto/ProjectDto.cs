using Sale_Saas.Application.Features.ProjectStatusFeature.Dto;
namespace Sale_Saas.Application.Features.ProjectFeature.Dto
{
    public class ProjectDto : BaseEntityDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Result { get; set; } = "";
        public string Type { get; set; } = "";
        public int Point { get; set; } = 0;
        public string Note { get; set; } = "";
        public string ApplicationUser { get; set; } = "";
        public string Service { get; set; } = "";
        public ProjectStatusDto ProjectStatus { get; set; } = new ProjectStatusDto();
        //public ProjectDto_Contract Contract { get; set; } = new ProjectDto_Contract();

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Contract, ProjectDto_Contract>();
                CreateMap<Project, ProjectDto>()
                    .ForMember(dest => dest.ProjectStatus, 
                        opt => opt.MapFrom(src => src.ProjectStatus != null ? src.ProjectStatus : new ProjectStatus()))
                    .ForMember(dest => dest.ApplicationUser, 
                        opt => opt.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.FullName ?? "" : ""));
            }
        }
    }

    public class ProjectDto_Contract : BaseEntityDto
    {
        public string Code { get; set; } = "";
        public string Number { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class ProjectUpdateDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; } 
        public string? Service { get; set; }
        public string? Type { get; set; }
        public Guid? ApplicationUserId { get; set; }
    }

    public class ProjectUpdateResultDto
    {
        public string? Result { get; set; }
        public int? Point { get; set; }
        public Guid? ProjectStatusId { get; set; }
        public string? Note { get; set; }

    }
}
