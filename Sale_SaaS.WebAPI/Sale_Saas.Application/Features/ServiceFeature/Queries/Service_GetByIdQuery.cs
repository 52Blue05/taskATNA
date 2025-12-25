using Sale_Saas.Application.Features.ServiceFeature.Dto;

namespace Sale_Saas.Application.Features.ServiceFeature.Queries
{
    public record Service_GetByIdQuery(Guid Id) : IRequest<Result<ServiceDto>>;
    public class Service_GetByIdQueryHandler : IRequestHandler<Service_GetByIdQuery, Result<ServiceDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Service_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ServiceDto>> Handle(Service_GetByIdQuery request, CancellationToken cancellationToken)
        {
            ServiceDto? Service = await (  from cv in _context.Services
                                           where cv.DeleteFlag != true && cv.Id == request.Id
                                           select new ServiceDto()
                                           {
                                               Id = cv.Id,
                                               Code = cv.Code ?? "",
                                               Name = cv.Name ?? "",
                                               ShortName = cv.ShortName ?? ""
                                           }).AsNoTracking().FirstOrDefaultAsync();
            return Result<ServiceDto>.Success(Service);
        }
    }
}
