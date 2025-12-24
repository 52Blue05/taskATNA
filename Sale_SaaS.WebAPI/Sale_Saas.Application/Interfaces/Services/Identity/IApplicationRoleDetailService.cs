using Sale_Saas.Application.Features.FeaturePermissionFeature.Dto;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Interfaces.Services.Identity
{
    public interface IApplicationRoleDetailService
    {
        Task<Result<string>> AddOrUpdateAsync(ApplicationRoleDetailRequest request);
        Task<Result<List<ApplicationRoleDetailDto>>> GetListByApplicationUserId(string applicationIserId);
        Task<Result<List<ApplicationRoleDetailDto>>> GetListByApplicationRoleId(string applicationRoleId);
	}
}
