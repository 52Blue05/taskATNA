using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Interfaces.Services.Identity
{
    public interface IApplicationRoleService
    {
        Task<List<ApplicationRole>> GetListRoleByUserId(Guid userId);
        Task<List<string>> GetListRoleStringByUserId(Guid userId, string connectionString);
        Task<List<string>> GetListRoleStringByUserIdAndRolePositionId(Guid userId, string rolePositionId, string connectionString);
        Task<Result<List<ApplicationRoleDto>>> GetAllQuery(GetAllQueryRequest request, Guid userId = default);
        Task<Result<PaginatedList<ApplicationRoleDto>>> GetListWithPaginationQuery(GetListRoleWithPaginationQueryRequest request);
        Task<Result<string>> DeleteByIds(DeleteRequest request);
        Task<Result<ApplicationRoleDto>> GetById(Guid id);
        Task<Result<List<ApplicationRoleDto>>> AddOrUpdateAsync(List<AddOrUpdateRequest> request);
        Task<Result<List<ApplicationRoleDto>>> FilterQuery(FilterQueryRequest request);
        Task<Result<Guid>> GetIdByRoleName(string roleName);
        Task<Result<ApplicationRole>> GetByRoleName(string roleName);
        Task<string> GetCurrentRoleOfUser(Guid userId);
        Task<Result<List<RoleDto>>> GetApplicationRoleByRolePositionId(string rolePositionId);
        Task<Result<List<RoleDto>>> GetListRoleSaleKitByRolePositionId(string rolePostionId);
        Task<Result<List<RoleDto>>> GetListRoleByUserIdHasResult(Guid userId);
        Task<string> GetRoleById(Guid id);
        Task<Result<List<RoleDto>>> GetListRoleByUser(Guid userId, string rolePositionId);
    }
}