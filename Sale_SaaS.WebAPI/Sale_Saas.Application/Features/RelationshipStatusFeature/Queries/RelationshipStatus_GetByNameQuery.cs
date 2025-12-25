using Sale_Saas.Application.Features.RelationshipStatusFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipStatusFeature.Queries
{
    public record RelationshipStatus_GetByNameQuery(string name) : IRequest<Result<RelationshipStatusDto>>;
    public class RelationshipStatus_GetByNameQueryHandler : IRequestHandler<RelationshipStatus_GetByNameQuery, Result<RelationshipStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RelationshipStatus_GetByNameQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<RelationshipStatusDto>> Handle(RelationshipStatus_GetByNameQuery request, CancellationToken cancellationToken)
        {
            RelationshipStatusDto? RelationshipStatus = await (from cv in _context.RelationshipStatuses
                                                               where cv.DeleteFlag != true && cv.Name == request.name
                                                               select new RelationshipStatusDto()
                                                               {
                                                                   Id = cv.Id,
                                                                   Code = cv.Code ?? "",
                                                                   Name = cv.Name ?? ""
                                                               }).AsNoTracking().FirstOrDefaultAsync();
            return Result<RelationshipStatusDto>.Success(RelationshipStatus);
        }
    }
}
