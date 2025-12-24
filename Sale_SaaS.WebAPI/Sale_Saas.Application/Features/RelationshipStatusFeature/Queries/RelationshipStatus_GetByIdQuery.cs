using Sale_Saas.Application.Features.RelationshipStatusFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipStatusFeature.Queries
{
    public record RelationshipStatus_GetByIdQuery(Guid Id) : IRequest<Result<RelationshipStatusDto>>;
    public class RelationshipStatus_GetByIdQueryHandler : IRequestHandler<RelationshipStatus_GetByIdQuery, Result<RelationshipStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RelationshipStatus_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<RelationshipStatusDto>> Handle(RelationshipStatus_GetByIdQuery request, CancellationToken cancellationToken)
        {
            RelationshipStatusDto? RelationshipStatus = await (from cv in _context.RelationshipStatuses
                                                               where cv.DeleteFlag != true && cv.Id == request.Id
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
