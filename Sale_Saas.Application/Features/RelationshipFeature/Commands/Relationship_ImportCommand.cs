using Sale_Saas.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.RelationshipFeature.Commands
{
    public record Relationship_ImportCommand(List<Relationship> RequestData) : IRequest<Result<List<Relationship>>>;
    public class Relationship_ImportCommandHandler : IRequestHandler<Relationship_ImportCommand, Result<List<Relationship>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Relationship_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }
        public async Task<Result<List<Relationship>>> Handle(Relationship_ImportCommand request, CancellationToken cancellationToken)
        {
            _context.Relationships.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<Relationship>>.Success(request.RequestData);
        }
    }
}
