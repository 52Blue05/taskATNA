using Sale_Saas.Application.Features.RelationshipCustomerFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipCustomerFeature.Queries
{
     public record RelationshipCustomer_GetAllQuery(GetAllQueryRequest DataRequest) : IRequest<Result<List<RelationshipCustomerDto>>>;

     public class RelationshipCustomer_GetAllQueryHandler : IRequestHandler<RelationshipCustomer_GetAllQuery, Result<List<RelationshipCustomerDto>>>
     {
          private readonly IApplicationDbContext _context;
          private readonly IMapper _mapper;

          public RelationshipCustomer_GetAllQueryHandler(IApplicationDbContext context, IMapper mapper)
          {
               _context = context;
               _mapper = mapper;
          }
          public async Task<Result<List<RelationshipCustomerDto>>> Handle(RelationshipCustomer_GetAllQuery request, CancellationToken cancellationToken)
          {
               var result = await _context.RelationshipCustomers.Where(x => x.DeleteFlag != true).ToListAsync();
               return Result<List<RelationshipCustomerDto>>.Success(_mapper.Map<List<RelationshipCustomerDto>>(result));
          }
     }
}
