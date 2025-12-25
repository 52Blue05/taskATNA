using Sale_Saas.Application.Features.OpportunityStatusFeature.Dto;

namespace Sale_Saas.Application.Features.OpportunityStatusFeature.Queries
{
    public record OpportunityStatus_GetByIdQuery(Guid Id) : IRequest<Result<OpportunityStatusDto>>;
    public class OpportunityStatus_GetByIdQueryHandler : IRequestHandler<OpportunityStatus_GetByIdQuery, Result<OpportunityStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OpportunityStatus_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<OpportunityStatusDto>> Handle(OpportunityStatus_GetByIdQuery request, CancellationToken cancellationToken)
        {
            OpportunityStatusDto? OpportunityStatus = await (from sup in _context.OpportunityStatuses
                                                             where sup.DeleteFlag != true && sup.Id == request.Id
                                                             select new OpportunityStatusDto()
                                                             {
                                                                 Id = sup.Id,
                                                                 Code = sup.Code ?? "",
                                                                 Name = sup.Name ?? ""
                                                             }).AsNoTracking().FirstOrDefaultAsync();
            return Result<OpportunityStatusDto>.Success(OpportunityStatus);
        }
    }
}
