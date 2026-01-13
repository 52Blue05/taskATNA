using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ContractFeature.Commands
{
    public record Contract_DeleteByIdCommand(Guid userId, DeleteRequest RequestData) : IRequest<Result<string>>;
    public class Contract_DeleteByIdCommandHandler : IRequestHandler<Contract_DeleteByIdCommand, Result<string>>
    {

        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Contract_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<string>> Handle(Contract_DeleteByIdCommand request, CancellationToken cancellationToken)
        {
            string result = string.Empty;

            if (request.RequestData.Ids == null) ExceptionHelper.RequestEmpty(request.RequestData.Locale);

			List<Guid> ids = request.RequestData.Ids!.Select(m => Guid.Parse(m)).ToList();
            var query = await _context.Contracts.Where(m => ids.Contains(m.Id)).ToListAsync();
            if (query == null || query.Count == 0) ExceptionHelper.NotFound(ids,request.RequestData.Locale);

			foreach (var item in query)
            {
                item.DeleteFlag = true;
                item.LastModifiedDate = DateTime.Now;
                item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
            }

            var eventLog = await _eventLogService.Create("ContractFeature", "ContractFeature",
                                                            "Contract_DeleteByIdCommand", request.userId);

            _context.Contracts.UpdateRange(query);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(string.Empty);
        }
    }
}
