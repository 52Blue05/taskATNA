namespace Sale_Saas.Application.Interfaces.Services.ApplicationService
{
    public interface IFeaturePermissionService
    {
        Task<string> GetPositionByUser(Guid UserId);
		Task<bool> ContainsPosition(Guid UserId,string position);
		Task<Dictionary<string, List<string>>> GetDictPermission(Guid Id);
        Task<bool> HasPermission(string menu, string feature, Guid user, bool? isThrow = false);
        Task<bool> ContainsPermission(string menu, List<string> features, Guid user, bool? isThrow = false);
        Task<List<string>> GetPolicy(Guid Id,string tenant);
    }
}
