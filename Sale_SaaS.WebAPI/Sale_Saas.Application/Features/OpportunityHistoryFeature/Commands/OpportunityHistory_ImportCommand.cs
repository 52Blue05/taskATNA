using Sale_Saas.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.OpportunityHistoryFeature.Commands
{
    public record OpportunityHistory_ImportCommand(List<OpportunityHistory> RequestData) : IRequest<Result<List<OpportunityHistory>>>;
    public class OpportunityHistory_ImportCommandHandler : IRequestHandler<OpportunityHistory_ImportCommand, Result<List<OpportunityHistory>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public OpportunityHistory_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }
        public async Task<Result<List<OpportunityHistory>>> Handle(OpportunityHistory_ImportCommand request, CancellationToken cancellationToken)
        {
            _context.OpportunityHistories.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<OpportunityHistory>>.Success(request.RequestData);
        }
    }
}
