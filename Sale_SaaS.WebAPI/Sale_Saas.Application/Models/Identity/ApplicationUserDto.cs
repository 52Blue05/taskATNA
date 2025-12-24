using Sale_Saas.Application.Features.ApplicationUserStatusFeature.Dto;
using Sale_Saas.Application.Models.Tenant;
using Sale_Saas.Domain.Entities.Tenant;

namespace Sale_Saas.Application.Models.Identity;
public class ApplicationUserDto
{
    public Guid? Id { set; get; }
    public string? UserName { get; set; }
    public string? PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Avatar { set; get; }
    public string? Email { set; get; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Code { get; set; }
    public string? CurrentPosition { get; set; }
    public string? CurrentTenant { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Review { get; set; }
    public ApplicationUserStatusDto? ApplicationUserStatus { set; get; }
    public Guid CreatedApplicationUserId { set; get; }
    public Guid LastModifiedApplicationUserId { set; get; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public bool SMS { set; get; }
    public string? Notes { set; get; }
    public IList<ApplicationRoleDto>? ApplicationRoles { get; set; }
    public bool? IsAdmin { get; set; } = false;
	public Guid? GroupTenantId { set; get; }
    public string? Color1 { get; set; }
    public string? Color2 { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UserStatus, ApplicationUserStatusDto>();
            CreateMap<ApplicationUserWithTenantDto, ApplicationUserDto>();
			CreateMap<User, ApplicationUserDto>().ForMember(dest => dest.ApplicationUserStatus, opt => opt.MapFrom(src => src.ApplicationUserStatus));
			CreateMap<ApplicationUser, ApplicationUserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => (src.FirstName + " " ?? "") + (src.LastName ?? "")))
                .ForMember(dest => dest.ApplicationUserStatus, opt => opt.MapFrom(src => src.ApplicationUserStatus))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber));
        }
    }

}

public class UserBasicInfoDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Code { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string Avatar { get; set; } = "";
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, UserBasicInfoDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber));
        }
    }
}

public class EmployeeUpdateDto
{
    public Guid Id { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Review { get; set; }
    public string? Notes { set; get; }
}

public class ApplicationUserWithTenantDto : ApplicationUserDto
{
    public List<TenantDto> Tenants { get; set; } = new List<TenantDto>();
	private class Mapping : Profile
	{
		public Mapping()
		{
            CreateMap<ApplicationUserDto, ApplicationUserWithTenantDto>();
		}
	}
}

public class UserMainTenantDto : BaseEntityDto
{
    public string? Email { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FullName { get; set; }
	public string? Phone { get; set; }
	public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
	public string? Password { get; set; }
    public string? Code { get; set; }
}

public class AddUserToTenantDto
{
    public Guid? Id { get; set; }
	public string? TenantId { get; set; } = "";
    public List<Guid>? ApplicationRoleIds { get; set; } = new List<Guid>();
}

public class UpdateUserToTenantDto
{
    public Guid? Id { get; set; }
    public string? TenantId { get; set; } = "";
    public List<Guid>? ApplicationRoleIds { get; set; } = new List<Guid>();
}

public class GetListUserForSyllabusDto
{
    public string? Email { get; set; }
}


public class CreatedUserDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
}

