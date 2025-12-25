using Sale_Saas.Application.Features.SaleKitFeature.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.SaleKitFeature.Queries
{
    public record SaleKit_GetListQuery(FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<SaleKitDto>>>;

    public class SaleKit_GetListQueryHandler : IRequestHandler<SaleKit_GetListQuery, Result<IEnumerable<SaleKitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SaleKit_GetListQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<SaleKitDto>>> Handle(SaleKit_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from sup in _context.SaleKits
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
						};

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Name.Contains(request.RequestData.TextSearch) ||
                                         x.OriginalFileName.Contains(request.RequestData.TextSearch));
            }

            if (request.RequestData.Skip != null)
            {
                query = query.Skip(request.RequestData.Skip.Value);
            }

            if (request.RequestData.TotalRecord != null)
            {
                query = query.Take(request.RequestData.TotalRecord.Value);
            }

            var data = await query.AsNoTracking().ToListAsync();

            return Result<IEnumerable<SaleKitDto>>.Success(data);
        }
    }
}
