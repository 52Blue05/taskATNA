namespace Sale_Saas.Application.Features.OpportunityFeature.Commands
{
    public record Opportunity_DeleteByIdCommand(DeleteRequest RequestData) : IRequest<Result<string>>;
    public class Opportunity_DeleteByIdCommandHandler : IRequestHandler<Opportunity_DeleteByIdCommand, Result<string>>
    {

        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public Opportunity_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(Opportunity_DeleteByIdCommand request, CancellationToken cancellationToken)
        {
            string result = string.Empty;

            if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
            List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();
            var query = await _context.Opportunities.Where(m => ids.Contains(m.Id)).ToListAsync();
            if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");

            foreach (var item in query)
            {
                item.DeleteFlag = true;
                item.LastModifiedDate = DateTime.Now;
                item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
            }

            _context.Opportunities.UpdateRange(query);

            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(string.Empty);
        }
    }
}
