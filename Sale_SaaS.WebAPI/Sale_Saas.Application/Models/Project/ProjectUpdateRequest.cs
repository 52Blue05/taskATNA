using Sale_Saas.Application.Features.ProjectFeature.Dto;

namespace Sale_Saas.Application.Models.Project
{
    public class ProjectUpdateRequest
    {
        public Guid? Id { set; get; }
        public ProjectUpdateDto? Data { set; get; }
        public Guid? CreatedApplicationUserId { set; get; }
        public Guid? LastModifiedApplicationUserId { set; get; }
    }
}
