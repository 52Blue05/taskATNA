using Sale_Saas.Application.Features.ServiceFeature.Dto;

namespace Sale_Saas.Application.Features.ServiceFeature.Queries
{
    public record Service_GetListQuery(FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<ServiceDto>>>;

    public class Service_GetListQueryHandler : IRequestHandler<Service_GetListQuery, Result<IEnumerable<ServiceDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Service_GetListQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ServiceDto>>> Handle(Service_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from cv in _context.Services
                        where cv.DeleteFlag != true
                        select new ServiceDto()
                        {
                            Id = cv.Id,
                            Code = cv.Code ?? "",
                            Name = cv.Name ?? "",
                            ShortName = cv.ShortName ?? ""
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Name.Contains(request.RequestData.TextSearch) ||
                                         x.ShortName.Contains(request.RequestData.TextSearch) ||
                                         x.Code.Contains(request.RequestData.TextSearch) );
            }

            if (!string.IsNullOrEmpty(request.RequestData.Code))
            {
                query = query.Where(x => x.Code.Contains(request.RequestData.Code));
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

            return Result<IEnumerable<ServiceDto>>.Success(data);
        }
    }
}
