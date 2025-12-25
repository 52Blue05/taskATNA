using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;

namespace Sale_Saas.Application.Features.OpportunityFeature.Commands
{
    public record Opportunity_UpdateStatusByIdCommand(UpdateStatusRequest RequestData) : IRequest<Result<OpportunityDto>>;
    public class Opportunity_UpdateStatusByIdCommandHandler : IRequestHandler<Opportunity_UpdateStatusByIdCommand, Result<OpportunityDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IApplicationUserService _UserService;
        private readonly IMapper _mapper;
        public Opportunity_UpdateStatusByIdCommandHandler(IMapper mapper, IApplicationDbContext context, IApplicationUserService UserService)
        {
            _context = context;
            _mapper = mapper;
            _UserService = UserService;
        }

        public async Task<Result<OpportunityDto>> Handle(Opportunity_UpdateStatusByIdCommand request, CancellationToken cancellationToken)
        {
            Opportunity data = await OpportunityService.GetOpportunity(request.RequestData.Id,_context);
            OpportunityStatus status = await OpportunityService.GetStatus(request.RequestData.Status, _context);

            data.OpportunityStatus = status;
            data.LastModifiedDate = DateTime.Now;
            data.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<OpportunityDto>.Success(_mapper.Map<OpportunityDto>(data));
        }
    }
}
