using Sale_Saas.Application.Features.BenefitHistoryFeature.Dto;
using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitHistoryFeature.Queries
{
	public record BenefitHistory_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<IEnumerable<BenefitHistoryDto>>>;
	public class BenefitHistory_GetByIdQueryHandler : IRequestHandler<BenefitHistory_GetByIdQuery, Result<IEnumerable<BenefitHistoryDto>>>
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public BenefitHistory_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
		{
			_context = context;
			_mapper = mapper;
			_eventLogService = eventLogService;
		}

		public async Task<Result<IEnumerable<BenefitHistoryDto>>> Handle(BenefitHistory_GetByIdQuery request, CancellationToken cancellationToken)
		{
			IEnumerable<BenefitHistoryDto> histories = (await (from his in _context.BenefitHistories
															   join user in _context.ApplicationUsers on his.ApplicationUserId equals user.Id
															   join pre in _context.BenefitStatuses on his.PreviousStatusId equals pre.Id
															   join upd in _context.BenefitStatuses on his.UpdatedStatusId equals upd.Id
															   where his.DeleteFlag != true && his.BenefitId == request.Id
															   orderby his.CreatedDate descending
															   select new BenefitHistoryDto()
															   {
																   Id = his.Id,
																   ApplicationUser = user.FullName ?? "",
																   PreviousStatus = new BenefitStatusDto
																   {
																	   Id = pre.Id,
																	   Code = pre.Code ?? "",
																	   Name = pre.Name ?? ""
																   },
																   UpdatedStatus = new BenefitStatusDto
																   {
																	   Id = upd.Id,
																	   Code = upd.Code ?? "",
																	   Name = upd.Name ?? ""
																   },
																   CreatedDate = his.CreatedDate
															   }).AsNoTracking().ToListAsync()).AsReadOnly();

            var eventLog = await _eventLogService.Create("BenefitHistoryFeature", "BenefitHistoryFeature", "BenefitHistory_GetByIdQuery", request.userId);

            return Result<IEnumerable<BenefitHistoryDto>>.Success(histories);
		}
	}
}
