using Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Dto;
using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Queries
{
    public record ApplicationRoleSaleKit_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<ApplicationRoleSaleKitDto>>>;
    public class ApplicationRoleSaleKit_GetAllQueryHandler : IRequestHandler<ApplicationRoleSaleKit_GetAllQuery, Result<IEnumerable<ApplicationRoleSaleKitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public ApplicationRoleSaleKit_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<ApplicationRoleSaleKitDto>>> Handle(ApplicationRoleSaleKit_GetAllQuery request, CancellationToken cancellationToken)
        {
            if (request.RequestData.RoleId == null)
            {
                throw new ApplicationException("Vui lòng chọn vị trí phân quyền");
            }
            var role = await _context.ApplicationRoles.FirstOrDefaultAsync(s => s.Id == request.RequestData.RoleId && !s.DeleteFlag);
            if (role == null) throw new ApplicationException("Bạn không có quyền thực hiện thao tác này");

            var salekits = await _context.ApplicationRole_SaleKits
                                       .Where(s => s.ApplicationRoleId == role.Id && !s.DeleteFlag)
                                       .Select(s => s.SaleKitId)
                                       .ToListAsync();

            var SaleKitNotExist = await _context.SaleKits
                                                .Where(s => !salekits.Contains(s.Id) && s.DeleteFlag != true)
                                                .ToListAsync();

            if (SaleKitNotExist.Any())
            {
                foreach (var item in SaleKitNotExist)
                {
                    ApplicationRoleSaleKit entity = new ApplicationRoleSaleKit()
                    {
                        Id = new Guid(),
                        SaleKitId = item.Id,
                        ApplicationRoleId = role.Id,
                        Access = false,
                        CreatedApplicationUserId = item.CreatedApplicationUserId,
                        LastModifiedApplicationUserId = item.LastModifiedApplicationUserId,
						CreatedDate = item.CreatedDate
					};
                    _context.ApplicationRole_SaleKits.Add(entity);
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            IEnumerable<ApplicationRoleSaleKitDto> datas = (await (from con in _context.ApplicationRole_SaleKits
                                                                   where con.DeleteFlag != true && con.ApplicationRoleId == role.Id
                                                                   join salekit in _context.SaleKits on con.SaleKitId equals salekit.Id
                                                                   select new ApplicationRoleSaleKitDto()
                                                                   {
                                                                       SaleKitId = con.SaleKitId ?? Guid.Empty,
                                                                       Access = con.Access ?? false,
                                                                       ApplicationRoleId = con.ApplicationRoleId ?? Guid.Empty,
                                                                       Name = salekit.Name ?? "",
                                                                       ParentId = salekit.ParentId
                                                                   }).AsNoTracking().ToListAsync()).AsReadOnly();

            var eventLog = await _eventLogService.Create("ApplicationRoleSaleKitFeature", "ApplicationRoleSaleKitFeature","ApplicationRoleSaleKit_GetAllQuery", request.userId);

            return Result<IEnumerable<ApplicationRoleSaleKitDto>>.Success(datas);
        }
    }
}
