using Sale_Saas.Application.Features.ApplicationUserStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;


namespace Sale_Saas.Application.Features.ApplicationUserStatusFeature.Queries
{
    public record ApplicationUserStatus_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<ApplicationUserStatusDto>>;

    public class ApplicationUserStatus_GetByIdQueryHandler : IRequestHandler<ApplicationUserStatus_GetByIdQuery, Result<ApplicationUserStatusDto>>
    {
        private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

		public ApplicationUserStatus_GetByIdQueryHandler(IMapper mapper, 
                                                            IApplicationDbContext context, IEventLogService eventLogService)
        {            
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<ApplicationUserStatusDto>> Handle(ApplicationUserStatus_GetByIdQuery request, CancellationToken cancellationToken)
        {
            var eventLog = await _eventLogService.Create("ApplicationUserStatusFeature", "ApplicationUserStatusFeature",
                                "ApplicationUserStatus_GetByIdQuery", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<ApplicationUserStatusDto>.Success(await (from cv in _context.ApplicationUserStatuses
                          where cv.DeleteFlag != true && cv.Id == request.Id
                          select new ApplicationUserStatusDto()
                          {
                              Id = cv.Id,
                              Code = cv.Code ?? string.Empty,
                              Name = cv.Name ?? string.Empty
                          })
                          .AsNoTracking()
                          .FirstOrDefaultAsync());
        }
	}
}
