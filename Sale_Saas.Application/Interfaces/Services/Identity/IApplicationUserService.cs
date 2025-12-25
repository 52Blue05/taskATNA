//using Sale_Saas.Application.Features.MenuFeature.Dto;
using Sale_Saas.Application.Models.Employee;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Interfaces.Services.Identity
{
    public interface IApplicationUserService
    {
		Task<bool> CheckIsAdmin(Guid id);
		Task<ApplicationUser> FindAsync(Guid? id);
        Task<Result<LoginDto>> Login(LoginRequest request, string tenantId, string groupTenantId);
		Task<Result<LoginDto>> LoginWithNoTenant(ApplicationUserWithTenantDto request);
		Task<Result<string>> ChangePasswordAsync(ChangePasswordRequest request);
		Task<Result<string>> ResetPasswordAsync(ResetPasswordRequest request);
		Task<Result<List<ApplicationUserDto>>> GetAllQueryAsync(GetAllQueryRequest request);
        Task<Result<PaginatedList<ApplicationUserDto>>> GetListWithPaginationQuery(GetListApplicationUserWithPaginationQueryRequest request);
		Task<Result<PaginatedList<ApplicationUserDto>>> GetUserInTenantPaginationQuery(GetListApplicationUserWithPaginationQueryRequest request);
		Task<Result<List<ApplicationUserDto>>> FilterQueryAsync(FilterQueryRequest request);
        Task<Result<ApplicationUserDto>> GetById(Guid id);
		Task<Result<ApplicationUserDto>> GetProfile(Guid id);
		Task<Result<ApplicationUserDto>> GetProfileAdmin(string emails);
		Task<Result<ApplicationUser>> GetUserById(Guid id);
        Task<List<Guid>> GetUserIdByRolePosition(string position);
		Task<List<Guid>> GetUserIdByRolePositionAndPermission(string position,string menu,string feature);
        Task<List<UserRoleDto>> GetUserRoleByRolePositionAndPermission(string position, string menu, string feature);

        Task<Result<string>> DeleteByIds(DeleteRequest request);
        Task<Result<string>> ApplicationRoleAssign(ApplicationRoleAssignRequest request);
        Task<Result<string>> SaveApplicationRolesAsync(ApplicationUserRequest request);
		Task<Result<ApplicationUserDto>> UpdateUserWithManyRole(ApplicationRoleAssignRequest request);
		Task<Result<ApplicationUserDto>> AddUserWithManyRole(ApplicationUserDto user,List<Guid>? roles);
		Task<Result<List<ApplicationUserDto>>> AddOrUpdateAsync(List<AddOrUpdateApplicationUserRequest> request, string sConnect = "");
		Task<Result<List<ApplicationUserDto>>> AddUserIntoTenant(List<AddOrUpdateApplicationUserRequest> request, string sConnect = "");
		Task<Result<string>> DeleteAvatarByApplicationUserId(string applicationUserId);
        //Task<List<MenuDto>> GetListMenuByApplicationUserId(string applicationUserId);
        Task<Result<string>> SaveApplicationRolesAdminAsync(ApplicationUserAdminRequest request);
        Task<Result<List<ApplicationUserDto>>> UpdateEmployeeAsync(List<ApplicationUserDto> request, Guid? ModifiedUser = null);
        Task<Result<List<ApplicationUserDto>>> UpdateReviewEmployeeAsync(List<AddOrUpdateRequest> request, Guid? ModifiedUser = null);
        Task<UserBasicInfoDto> GetUserBasicById(Guid id);
        Task<Result<string>> GetUserIdByCode(string code);
        Task<Result<Guid>> GetUserIdByEmail(string email);
        Task<Result<ApplicationUser>> GetUserByEmail(string email);
        Task<Result<List<string>>> GetConnectStrByEmail(string email);
        Task<Result<string>> GetTenantIdByConnStr(string connStr);
        Task<List<ApplicationRoleDto>> GetApplicationRolesByUserIdAsync(Guid userId);
        Task<CreatedUserDto> GetUserBasicInforById(Guid userId);
        Task<string> GetAvatar(Guid userId);
        void SetConnectDB(string connectString);
        void ClearConnectDB();
        string RefreshToken(string jwt, int exp);
        string GenerateRefreshToken();
        Task<Result<bool>> Logout();
        DateTime GetValidTo(string jwt);
        void DropDB();
    }
}