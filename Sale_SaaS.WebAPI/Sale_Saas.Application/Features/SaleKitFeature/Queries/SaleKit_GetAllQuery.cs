using Sale_Saas.Application.Features.SaleKitFeature.Dto;
namespace Sale_Saas.Application.Features.SaleKitFeature.Queries;

public record SaleKit_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<SaleKitDto>>>;
public class SaleKit_GetAllQueryHandler : IRequestHandler<SaleKit_GetAllQuery, Result<IEnumerable<SaleKitDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SaleKit_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<SaleKitDto>>> Handle(SaleKit_GetAllQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<SaleKitDto> salekits = (await (from sup in _context.SaleKits
                                                   where sup.DeleteFlag != true
                                                   select new SaleKitDto()
                                                   {
														   Id = sup.Id,
														   OriginalFileName = sup.OriginalFileName ?? "",
														   FileName = sup.FileName ?? "",
														   ContentType = sup.ContentType ?? "",
														   FilePath = sup.FilePath ?? "",
														   Type = sup.Type ?? "",
														   Extension = sup.Extension ?? "",
														   ParentId = sup.ParentId,
														   ServerPath = sup.ServerPath ?? "",
														   FileSize = sup.FileSize ?? 0,
														   FolderName = sup.FolderName ?? "",
														   Name = sup.Name ?? "",
														   Description = sup.Description ?? "",
                                                       CreatedDate = sup.CreatedDate
													   }).AsNoTracking().ToListAsync()).AsReadOnly();

        return Result<IEnumerable<SaleKitDto>>.Success(salekits);
    }
}
