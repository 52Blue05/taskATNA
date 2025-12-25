using Sale_Saas.Application.Features.OpportunityStatusFeature.Dto;
using Sale_Saas.Domain.Enums;
using static Sale_Saas.Application.Features.OpportunityStatusFeature.Requests;

namespace Sale_Saas.Application.Features.OpportunityStatusFeature.Queries
{
    public record OpportunityStatus_GetAllQuery(OpportunityStatusGetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<OpportunityStatusDto>>>;
    public class OpportunityStatus_GetAllQueryHandler : IRequestHandler<OpportunityStatus_GetAllQuery, Result<IEnumerable<OpportunityStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OpportunityStatus_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<OpportunityStatusDto>>> Handle(OpportunityStatus_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<OpportunityStatusDto> list = (await (from sup in _context.OpportunityStatuses
                                                             where sup.DeleteFlag != true
                                                             select new OpportunityStatusDto()
                                                             {
                                                                 Id = sup.Id,
                                                                 Code = sup.Code ?? "",
                                                                 Name = sup.Name ?? ""
                                                             }).AsNoTracking().ToListAsync()).AsReadOnly();

            if (request.RequestData.StatusCode != null && request.RequestData.StatusCode == OpportunityStatusEnum.CLOSE.ToString())
            {
                list = list.Where(s => s.Code != OpportunityStatusEnum.PENDING.ToString()).ToList();
            }

            return Result<IEnumerable<OpportunityStatusDto>>.Success(list);
        }
    }
}
