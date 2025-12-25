using Sale_Saas.Application.Features.SaleKitFeature.Dto;

namespace Sale_Saas.Application.Features.SaleKitFeature.Queries
{
    public record SaleKit_GetByIdQuery(Guid Id) : IRequest<Result<SaleKitDto>>;
    public class SaleKit_GetByIdQueryHandler : IRequestHandler<SaleKit_GetByIdQuery, Result<SaleKitDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SaleKit_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<SaleKitDto>> Handle(SaleKit_GetByIdQuery request, CancellationToken cancellationToken)
        {
            SaleKitDto? SaleKit = await (from sup in _context.SaleKits
                                         where sup.DeleteFlag != true && sup.Id == request.Id
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
										 }).AsNoTracking().FirstOrDefaultAsync();
            return Result<SaleKitDto>.Success(SaleKit);
        }
    }
}
