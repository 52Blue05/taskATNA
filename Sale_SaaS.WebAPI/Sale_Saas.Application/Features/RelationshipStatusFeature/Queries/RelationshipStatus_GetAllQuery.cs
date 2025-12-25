using Sale_Saas.Application.Features.RelationshipStatusFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipStatusFeature.Queries
{
    public record RelationshipStatus_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<RelationshipStatusDto>>>;
    public class RelationshipStatus_GetAllQueryHandler : IRequestHandler<RelationshipStatus_GetAllQuery, Result<IEnumerable<RelationshipStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RelationshipStatus_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<RelationshipStatusDto>>> Handle(RelationshipStatus_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<RelationshipStatusDto> customers = (await (from cv in _context.RelationshipStatuses
                                                                   where cv.DeleteFlag != true
                                                                   select new RelationshipStatusDto()
                                                                   {
                                                                       Id = cv.Id,
                                                                       Code = cv.Code ?? "",
                                                                       Name = cv.Name ?? ""
                                                                   }).AsNoTracking().ToListAsync()).AsReadOnly();

            return Result<IEnumerable<RelationshipStatusDto>>.Success(customers);
        }
    }
}
