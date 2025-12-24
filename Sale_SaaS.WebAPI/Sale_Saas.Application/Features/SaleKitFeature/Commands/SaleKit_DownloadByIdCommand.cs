using Sale_Saas.Application.Features.SaleKitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.SaleKit;
using System.Data;

namespace Sale_Saas.Application.Features.SaleKitFeature.Commands
{
    public record SaleKit_DownloadByIdCommand(SaleKitDownloadRequest RequestData) : IRequest<Result<string>>;

    public class SaleKit_DownloadByIdCommandHandler : IRequestHandler<SaleKit_DownloadByIdCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IFileStorageService _storageService;
        private readonly IApplicationRoleService _roleService;
        public SaleKit_DownloadByIdCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IFileStorageService storageService,
                                                IApplicationRoleService roleService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _storageService = storageService;
            _roleService = roleService;
        }

        public async Task<Result<string>> Handle(SaleKit_DownloadByIdCommand request, CancellationToken cancellationToken)
        {
            var salekit = await _context.SaleKits.Include(s => s.RoleSaleKits)
                                .FirstOrDefaultAsync(s => s.Id == request.RequestData.Id);
            if (salekit == null)
            {
                throw new ApplicationException($"Không tìm thấy SaleKit với Id: {request.RequestData.Id}");
            }
            if (string.IsNullOrEmpty(salekit.FileName) || string.IsNullOrEmpty(salekit.FolderName))
            {
                throw new ApplicationException($"Tài liệu không tồn tại media");
            }

            //var roles = (await _roleService.GetListRoleByUserId(request.RequestData.UserId)).Select(s => s.Id).ToList();
            //if (roles.Count == 0 || salekit.RoleSaleKits == null || salekit.RoleSaleKits.Count == 0)
            //    throw new ApplicationException($"Bạn không có quyền truy cập tài liệu này");

            //var access = salekit.RoleSaleKits
            //                    .Where(s => s.ApplicationRoleId != null && roles.Contains((Guid)s.ApplicationRoleId) && !s.DeleteFlag && s.Access == true)
            //                    .FirstOrDefault();

            //if (access == null)
            //    throw new ApplicationException($"Bạn không có quyền truy cập tài liệu này");

            //var response = await _storageService.DownloadFileAsync(salekit.FileName, salekit.FolderName);
            //SaleKitDownloadDto result = new SaleKitDownloadDto
            //{
            //    Bytes = response.data,
            //    SaleKit = salekit
            //};

            return Result<string>.Success(salekit.ServerPath);
        }
    }
}
