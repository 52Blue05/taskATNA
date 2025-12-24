using Sale_Saas.Application.Features.RelationshipCustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.RelationshipFeature.Queries;

public record RelationshipMobile_GetListCustomerQuery(Guid UserId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<RelationshipCustomerDto>>>;

public class RelationshipMobile_GetListCustomerQueryHandler : IRequestHandler<RelationshipMobile_GetListCustomerQuery, Result<PaginatedList<RelationshipCustomerDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IMapper _mapper;

    public RelationshipMobile_GetListCustomerQueryHandler(IApplicationDbContext context, IEventLogService eventLogService, IMapper mapper)
    {
        _context = context;
        _eventLogService = eventLogService;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<RelationshipCustomerDto>>> Handle(RelationshipMobile_GetListCustomerQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        var listRelationshipCustomer = await _context.RelationshipCustomers.Where(x => x.DeleteFlag != true).AsNoTracking().ToListAsync();

        var result = _mapper.Map<List<RelationshipCustomerDto>>(listRelationshipCustomer);

        return Result<PaginatedList<RelationshipCustomerDto>>.Success(new PaginatedList<RelationshipCustomerDto>(result, result.Count, request.RequestData.PageIndex, request.RequestData.PageSize));
    }
}
