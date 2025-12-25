using Microsoft.Extensions.Configuration;
using Sale_Saas.Domain.Constants.API;
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
		services.Configure<APISystemInfoConstant>(configuration.GetSection("APISystemInfoConstant"));
		services.Configure<ApplicationUserStatusConstant>(configuration.GetSection("ApplicationUserStatus"));
		services.Configure<DataTypeFormatConstant>(configuration.GetSection("DataTypeFormat"));
		services.Configure<ImageConstant>(configuration.GetSection("Image"));
		services.Configure<PermisionTypeConstant>(configuration.GetSection("PermisionType"));
		services.Configure<LoaiThuMucTapTinConstant>(configuration.GetSection("LoaiThuMucTapTin"));		
		services.Configure<MailServiceConstant>(configuration.GetSection("MailService"));
		services.Configure<SystemLogInfoConstant>(configuration.GetSection("SystemLogInfo"));

		return services;
    }
}