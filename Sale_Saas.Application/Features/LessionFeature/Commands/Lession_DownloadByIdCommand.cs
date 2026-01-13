using Sale_Saas.Application.Features.LessionFeature.Dto;
using Sale_Saas.Application.Features.SaleKitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Models.Lession;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.LessionFeature.Commands;

public record Lession_DownloadByIdCommand(LessionDownloadRequest RequestData) : IRequest<Result<string>>;

public class Lession_DownloadByIdCommandHandler : IRequestHandler<Lession_DownloadByIdCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IFileStorageService _storageService;
    
    public Lession_DownloadByIdCommandHandler(IApplicationDbContext context, IMapper mapper, IInternalService internalService, IFileStorageService storageService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _storageService = storageService;
    }

    public async Task<Result<string>> Handle(Lession_DownloadByIdCommand request, CancellationToken cancellationToken)
    {
        var lession = await _context.Lessions.Where(x => x.Id == request.RequestData.Id && x.DeleteFlag != true)
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync();

        if (lession == null)
        {
            throw new ApplicationException("Không tìm thấy bài học cần download");
        }

        if (string.IsNullOrEmpty(lession.FileName))
        {
            throw new ApplicationException("Tài liệu không tồn tại trên media");
        }

        //var storageResult = await _storageService.DownloadFileAsync(lession.FileName);

        //result.Bytes = storageResult.data;
        //result.Lession = lession;

        return Result<string>.Success(lession.ServerPath);
    }
}