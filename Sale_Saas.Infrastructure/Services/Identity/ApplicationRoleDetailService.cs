using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Infrastructure.Services.Identity;

public class ApplicationRoleDetailService : IApplicationRoleDetailService
{
    private readonly ApplicationDbContext _context;

    public ApplicationRoleDetailService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> AddOrUpdateAsync(ApplicationRoleDetailRequest request)
    {
        ApplicationRoleDetail? applicationRoleDetail = await _context.ApplicationRoleDetails.FirstOrDefaultAsync(m => m.MenuId == request.MenuId
                                                                                                                    && m.ApplicationRoleId == request.ApplicationRoleId
                                                                                                                    && m.Permission == request.Permission);

        if (applicationRoleDetail == null)
        {
            _context.ApplicationRoleDetails.Add(new ApplicationRoleDetail()
            {
                ApplicationRoleId = request.ApplicationRoleId,
                MenuId = request.MenuId,
                Permission = request.Permission,
                LastModifiedApplicationUserId = request.CreatedApplicationUserId,
                CreatedApplicationUserId = request.CreatedApplicationUserId
            });
        }

        await _context.SaveChangesAsync();
        return Result<string>.Success(string.Empty);
    }
    public async Task<Result<List<ApplicationRoleDetailDto>>> GetListByApplicationUserId(string applicationIserId)
    {
        var queryApplicationRoleDetails = from rd in _context.ApplicationRoleDetails
                               //join m in _context.Menus on rd.MenuId equals m.Id
                               join r in _context.Roles on rd.ApplicationRoleId equals r.Id
                               join ur in _context.UserRoles on r.Id equals ur.RoleId
                               where ur.UserId == Guid.Parse(applicationIserId)
                                     //  && !string.IsNullOrEmpty(m.TenController)
                                     //  && !string.IsNullOrEmpty(m.TenAction)
                               select new { rd
                               //, m 
                               };

        var applicationRoleDetails = await queryApplicationRoleDetails.Select(x => new ApplicationRoleDetailDto()
        {
            //TenController = x.m.TenController,
            //TenAction = x.m.TenAction,
            Permision = x.rd.Permission
        }).AsNoTracking().ToListAsync();

        return Result<List<ApplicationRoleDetailDto>>.Success(applicationRoleDetails);
    }

    public async Task<Result<List<ApplicationRoleDetailDto>>> GetListByApplicationRoleId(string applicationRoleId)
    {
        var queryApplicationRoleDetails = from rd in _context.ApplicationRoleDetails
                               where rd.ApplicationRoleId == Guid.Parse(applicationRoleId)
                               select new { rd };

        var applicationRoleDetails = await queryApplicationRoleDetails.Select(x => new ApplicationRoleDetailDto()
        {
            MenuId = x.rd.MenuId,
            Permision = x.rd.Permission
        }).AsNoTracking().ToListAsync();

        return Result<List<ApplicationRoleDetailDto>>.Success(applicationRoleDetails);
    }
}
