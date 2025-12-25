namespace Sale_Saas.Application.Interfaces.Services;

public interface IApplicationDbContextInitialiser
{
	Task SeedAsync(string sConnect, List<string> listMenuActive = null);
	Task<int> InitMenu(List<string> listMenuActive = null);
	Task<int> InitFeature();
	Task<int> InitFeatureMenu();
	Task<int> InitRolePositionFeatureMenu();
	Task<int> InitApplicationRoleDetail();
	Task<bool> RemoveRolePositionFeatureMenu(string sConnect);
}
