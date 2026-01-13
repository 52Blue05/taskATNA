using Microsoft.AspNetCore.Http;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public interface IUserService
    {
        Task<Guid> GetApplicationUserIdByTenant(string username,string tenantId);
        Result<List<UserTenant>> CreateUser(CreateUser request, bool isCreateTenant = false);
        User GetUser(Guid id);
        PaginatedList<User> GetListUser(GetListWithPaginationQueryRequest request);
        UserTenant GetUserTenant(string userName, string tenantId);
        List<UserTenantDto> ListUserTenant(Guid id, bool cnStr = true);
        Task<List<UserTenantMobileDto>> ListUserTenantByMobile(Guid id, bool cnStr = true);
        Task<Result<UserLoginDto>> LoginUser(string userName, string password, Guid? groupTenantId = null, string deviceId = "");
        User GetUserAdminByUserName(string userName);
        bool UpdateUserTenant(string userName, string tenantId, Guid userId);
        List<AllUserTenantDto> ListAllUserTenant(Guid id, int? PageIndex = null, int? PageSize = null);
		Task<Result<string>> DeleteByIds(DeleteRequest request);
		Task<Result<string>> DeleteFromTenantByIds(DeleteRequest request);
		Task<Result<List<ApplicationUserWithTenantDto>>> GetListWithFilterQuery(FilterQueryRequest request);
		Task<Result<List<ApplicationUserWithTenantDto>>> GetRecommendUserQuery(FilterQueryRequest request);
		Task<Result<PaginatedList<ApplicationUserWithTenantDto>>> GetListWithPaginationQuery(GetListApplicationUserWithPaginationQueryRequest request);
        Task<Result<ApplicationUserWithTenantDto>> UpdateStatus(UpdateStatusRequest request);
		Task<Result<bool>> UpdatePassword(UpdatePasswordRequest request,bool checkCurrentPassword = true);
        Task<Result<bool>> UpdatePasswordByMobile(UpdatePasswordMobileRequest request, Guid userId, bool checkCurrentPassword = true);
        Task<Result<bool>> UpdatePasswordByOtpMobile(UpdatePasswordByOtpMobileRequest request, Guid userId, bool checkCurrentPassword = true);
        Task<Result<ApplicationUserWithTenantDto>> GetById(Guid id);
        Task<Result<List<ApplicationUserWithTenantDto>>> AddUser(List<AddOrUpdateRequest> request, Guid? groupTenant = null, Guid? userId = null);
		Task<Result<ApplicationUserWithTenantDto>> AddToTenant(AddUserToTenantDto request, Guid? userId = null);
		Task<Result<List<ApplicationUserWithTenantDto>>> UpdateUser(List<AddOrUpdateRequest> request, Guid? userId = null);
        Task<bool> CheckUserExisted(string tenantId);
        Task<User> GetUserByEmail(string email, Guid? groupTenantId = null, string deviceId = "", bool isLogout = false);
        Task<string> UpdatePasswordGen(User user);
        Task<Result<string>> UpdateAvatar(User user, IFormFile avatar);
        Task<Result<User>> UpdateProfile(UpdateProfileRequest request);
        Task<Result<UserDto>> GetUserById(Guid userId);
        Task<Result<Guid>> RegisterUser(UserMainTenantDto obj, UserStatus? status = null, Guid? groupTenant = null, Guid? userId = null, bool? isHasOtp = false, string? otpCode = "", string? deviceId = "");
        bool UpdateRefreshTokenUser(Guid? userId, string? refreshToken);
        User GetUserByRefreshToken(string refreshToken);
        Task<Result<string>> UpdateAvatarByMobile(Guid userId, IFormFile avatar);
        Task<Result<List<UserStatus>>> GetAllStatus();
        Task<Result<string>> MobileDelete(Guid userId);
    }
}
