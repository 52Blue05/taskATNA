using Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.SaleKit;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Commands
{
    public record ApplicationRoleSaleKit_AddOrUpdateCommand(Guid userId, SaleKitUpdateRequest RequestData) : IRequest<Result<List<ApplicationRoleSaleKitDto>>>;

    public class ApplicationRoleSaleKit_AddOrUpdateCommandHandler : IRequestHandler<ApplicationRoleSaleKit_AddOrUpdateCommand, Result<List<ApplicationRoleSaleKitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public ApplicationRoleSaleKit_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<ApplicationRoleSaleKitDto>>> Handle(ApplicationRoleSaleKit_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            if (request.RequestData.ApplicationRoleId == Guid.Empty)
            {
                throw new ApplicationException("Role Id không được phép null");
            }
            await ResetAllSaleKitByApplicationRole(request.RequestData.ApplicationRoleId, cancellationToken);

            if (request.RequestData.Data == null || !request.RequestData.Data.Any())
            {
                return Result<List<ApplicationRoleSaleKitDto>>.Success([]);
            }

            string actionName = string.Empty;
            foreach (var item in request.RequestData.Data)
            {
                if (item.Type == SaleKitTypeEnum.FOLDER.ToString())
                {
                    actionName = "HandleFolderAccess";

                    // grant access for parent folder (if it has) and 
                    await HandleFolderAccess(item, request.RequestData.CreatedApplicationUserId, request.RequestData.LastModifiedApplicationUserId);
                }
                else
                {
                    actionName = "HandleFileAccess";

                    await HandleFileAccess(item, request.RequestData.CreatedApplicationUserId, request.RequestData.LastModifiedApplicationUserId);
                }
            }

            var eventlog = await _eventLogService.Create("ApplicationRoleSaleKitFeature", "ApplicationRoleSaleKit_AddOrUpdateCommand"
                                                        , actionName, request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<ApplicationRoleSaleKitDto>>.Success(request.RequestData.Data);
        }

        private async Task ResetAllSaleKitByApplicationRole(Guid ApplicationRoleId, CancellationToken cancellationToken)
        {
            var listSaleKitHasAccessByRole = await _context.ApplicationRole_SaleKits.Where(x => x.ApplicationRoleId == ApplicationRoleId && x.DeleteFlag != true)
                                                                                    .ToListAsync();

            foreach (var item in listSaleKitHasAccessByRole)
            {
                _context.ApplicationRole_SaleKits.Remove(item);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task HandleFolderAccess(ApplicationRoleSaleKitDto item, Guid? createdByUserId, Guid? lastModifiedByUserId)
        {
            // find sale_kit
            var folder = await _context.SaleKits.FirstOrDefaultAsync(s => s.Id == item.SaleKitId && s.DeleteFlag != true);

            if (folder == null)
            {
                throw new ApplicationException($"Không tìm thấy dữ liệu: {item.SaleKitId}");
            }

            // Grant access to the parent of that sale kit
            await GrantAccessToParents(folder, item.ApplicationRoleId, item.Access, createdByUserId, lastModifiedByUserId);

            // grant access to that folder and the children of that sale kit
            await GrantAccessToFolderAndChildren(folder, item.ApplicationRoleId, item.Access, createdByUserId, lastModifiedByUserId);
        }

        private async Task GrantAccessToFolderAndChildren(SaleKit folder, Guid roleId, bool access, Guid? createdByUserId, Guid? lastModifiedByUserId)
        {
            await GrantAccess(folder, roleId, access, createdByUserId, lastModifiedByUserId);

            var childrens = await _context.SaleKits.Where(s => s.ParentId == folder.Id && s.DeleteFlag != true)
                                                   .ToListAsync();

            foreach (var child in childrens)
            {
                await GrantAccessToFolderAndChildren(child, roleId, access, createdByUserId, lastModifiedByUserId);
            }
        }

        private async Task HandleFileAccess(ApplicationRoleSaleKitDto item, Guid? createdByUserId, Guid? lastModifiedByUserId)
        {
            // find sale_kit
            var file = await _context.SaleKits.Where(s => s.Id == item.SaleKitId && s.DeleteFlag != true)
                                              .FirstOrDefaultAsync();

            if (file == null)
            {
                throw new ApplicationException("Không tìm được sale kit này trong dữ liệu");
            }

            // grant access to that specific sale kit
            await GrantAccess(file, item.ApplicationRoleId, item.Access, createdByUserId, lastModifiedByUserId);

            // Grant access to the parent of that sale kit
            await GrantAccessToParents(file, item.ApplicationRoleId, item.Access, createdByUserId, lastModifiedByUserId);
        }

        private async Task GrantAccess(SaleKit saleKit, Guid roleId, bool access, Guid? createdByUserId, Guid? lastModifiedByUserId)
        {
            var existingAccess = await _context.ApplicationRole_SaleKits.Where(s => s.SaleKitId == saleKit.Id
                                                                            && s.ApplicationRoleId == roleId
                                                                            && s.DeleteFlag != true)
                                                                        .FirstOrDefaultAsync();

            if (existingAccess == null)
            {
                var newAccess = new ApplicationRoleSaleKit
                {
                    Access = access,
                    ApplicationRoleId = roleId,
                    SaleKitId = saleKit.Id,
                    CreatedApplicationUserId = createdByUserId,
                    LastModifiedApplicationUserId = lastModifiedByUserId
                };

                _context.ApplicationRole_SaleKits.Add(newAccess);
            }
            else
            {
                existingAccess.Access = access;
                existingAccess.LastModifiedApplicationUserId = lastModifiedByUserId;

                _context.ApplicationRole_SaleKits.Update(existingAccess);
            }
        }

        private async Task GrantAccessToParents(SaleKit? saleKit, Guid roleId, bool access, Guid? createdByUserId, Guid? lastModifiedByUserId)
        {
            saleKit = await _context.SaleKits.Where(s => saleKit != null
                                                 && saleKit.ParentId != null
                                                 && s.Id == saleKit.ParentId
                                                 && s.DeleteFlag != true)
                                             .FirstOrDefaultAsync();

            while (saleKit != null)
            {
                await GrantAccess(saleKit, roleId, access, createdByUserId, lastModifiedByUserId);

                saleKit = await _context.SaleKits.Where(s => saleKit != null && s.Id == saleKit.ParentId && s.DeleteFlag != true)
                                                 .FirstOrDefaultAsync();
            }
        }
    }
}
