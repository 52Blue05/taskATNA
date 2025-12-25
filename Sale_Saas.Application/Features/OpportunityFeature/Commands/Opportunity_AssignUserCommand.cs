using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Commands
{
    public record Opportunity_AssignUserCommand(AssignUserRequest RequestData) : IRequest<Result<OpportunityDto>>;
    public class Opportunity_AssignUserCommandHandler : IRequestHandler<Opportunity_AssignUserCommand, Result<OpportunityDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IApplicationUserService _userService;
        private readonly IMapper _mapper;
        public Opportunity_AssignUserCommandHandler(IMapper mapper, IApplicationDbContext context, IApplicationUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<Result<OpportunityDto>> Handle(Opportunity_AssignUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userService.FindAsync(request.RequestData.ApplicationUserId);
            var data = await OpportunityService.GetOpportunity(request.RequestData.Id, _context);

            if (data.OpportunityStatus.Code == OpportunityStatusEnum.PENDING.ToString())
            {
                var status = await OpportunityService.GetStatus(OpportunityStatusEnum.ACTIVE.ToString(), _context);
                data.OpportunityStatusId = status.Id;
                data.OpportunityStatus = status;
            }

            data.OpportunityStartDate = request.RequestData.OpportunityStartDate;
            data.OpportunityEndDate = request.RequestData.OpportunityEndDate;
            data.ApplicationUser = user;
            data.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
            data.LastModifiedDate = DateTime.Now;
            data.ApplicationRoleId = request.RequestData.ApplicationRoleId;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<OpportunityDto>.Success(_mapper.Map<OpportunityDto>(data));
        }
    }
}
