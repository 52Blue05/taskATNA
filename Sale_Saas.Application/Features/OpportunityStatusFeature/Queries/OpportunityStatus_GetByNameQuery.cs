using Sale_Saas.Application.Features.OpportunityStatusFeature.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.OpportunityStatusFeature.Queries
{
    public record OpportunityStatus_GetByNameQuery(string name) : IRequest<Result<OpportunityStatusDto>>;
    public class OpportunityStatus_GetByNameQueryHandler : IRequestHandler<OpportunityStatus_GetByNameQuery, Result<OpportunityStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OpportunityStatus_GetByNameQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<OpportunityStatusDto>> Handle(OpportunityStatus_GetByNameQuery request, CancellationToken cancellationToken)
        {
            OpportunityStatusDto? OpportunityStatus = await (from sup in _context.OpportunityStatuses
                                                             where sup.DeleteFlag != true && sup.Name == request.name
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
