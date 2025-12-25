using Sale_Saas.Application.Features.ServiceFeature.Dto;

namespace Sale_Saas.Application.Features.ServiceFeature.Queries
{
    public record Service_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<ServiceDto>>>;
    public class Service_GetAllQueryHandler : IRequestHandler<Service_GetAllQuery, Result<IEnumerable<ServiceDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Service_GetAllQueryHandler(IMapper mapper,IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ServiceDto>>> Handle(Service_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<ServiceDto> customers = (await (from cv in _context.Services
                                                         where cv.DeleteFlag != true
                                                         select new ServiceDto()
                                                         {
                                                             Id = cv.Id,
                                                             Code = cv.Code ?? "",
                                                             Name = cv.Name ?? "",
                                                             ShortName = cv.ShortName ?? ""
                                                         }).AsNoTracking().ToListAsync()).AsReadOnly();

            return Result<IEnumerable<ServiceDto>>.Success(customers);
        }
    }
}
