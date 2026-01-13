using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.SaleKitFeature.Commands
{
    public record SaleKit_DeleteByIdCommand(DeleteRequest RequestData) : IRequest<Result<string>>;
    public class SaleKit_DeleteByIdCommandHandler : IRequestHandler<SaleKit_DeleteByIdCommand, Result<string>>
    {

        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _storageService;
        public SaleKit_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context, IFileStorageService storageService)
        {
            _context = context;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<Result<string>> Handle(SaleKit_DeleteByIdCommand request, CancellationToken cancellationToken)
        {
            string result = string.Empty;

            if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
            List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();
            var query = await _context.SaleKits.Include(s => s.RoleSaleKits).Where(m => ids.Contains(m.Id)).ToListAsync();
            if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");

            foreach (var item in query)
            {
                item.DeleteFlag = true;
                item.LastModifiedDate = DateTime.Now;
                item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;

                if(item.RoleSaleKits != null)
                {
                    foreach(var access in item.RoleSaleKits)
                    {
                        access.DeleteFlag = true;
                        access.LastModifiedDate = DateTime.Now;
                        access.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
                    }
                    _context.ApplicationRole_SaleKits.UpdateRange(item.RoleSaleKits);
                }

                if (!StringExtension.IsNullOrEmpty(new string?[] { item.FileName, item.FolderName }))
                {
                    await _storageService.DeleteMediaAsync(item.FileName ?? string.Empty, item.FolderName ?? string.Empty);
                }
            }

            _context.SaleKits.UpdateRange(query);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(string.Empty);
        }
    }
}
