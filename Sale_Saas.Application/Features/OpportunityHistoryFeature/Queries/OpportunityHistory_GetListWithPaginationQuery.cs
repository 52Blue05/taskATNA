using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.OpportunityHistoryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Opportunity;

namespace Sale_Saas.Application.Features.OpportunityHistoryFeature.Queries
{
    public record OpportunityHistory_GetListWithPaginationQuery(OpportunityGetByIdRequest RequestData) : IRequest<Result<PaginatedList<OpportunityHistoryDto>>>;

    public class OpportunityHistory_GetListWithPaginationQueryHandler : IRequestHandler<OpportunityHistory_GetListWithPaginationQuery, Result<PaginatedList<OpportunityHistoryDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IApplicationRoleService _roleService;
        public OpportunityHistory_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService)
        {
            _context = context;
            _mapper = mapper;
            _roleService = roleService;
        }

        public async Task<Result<PaginatedList<OpportunityHistoryDto>>> Handle(OpportunityHistory_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.OpportunityHistories
                                .Where(m => m.DeleteFlag != true)
                                .Include(s => s.ApplicationUser)
                                .OrderByDescending(x => x.CreatedDate)
                                .ProjectTo<OpportunityHistoryDto>(_mapper.ConfigurationProvider)
                                .AsNoTracking();

            if(request.RequestData.OpportunityId != null)
            {
                query = query.Where(s => s.OpportunityId == request.RequestData.OpportunityId);
            }

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Goal.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         x.ApplicationUser.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
            }

            return Result<PaginatedList<OpportunityHistoryDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
