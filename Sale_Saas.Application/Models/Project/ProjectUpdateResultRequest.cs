using Sale_Saas.Application.Features.ProjectFeature.Dto;

namespace Sale_Saas.Application.Models.Project
{
    public class ProjectUpdateResultRequest
    {
        public Guid? Id { set; get; }
        public ProjectUpdateResultDto? Data { set; get; }
        public Guid? CreatedApplicationUserId { set; get; }
        public Guid? LastModifiedApplicationUserId { set; get; }
    }
}
