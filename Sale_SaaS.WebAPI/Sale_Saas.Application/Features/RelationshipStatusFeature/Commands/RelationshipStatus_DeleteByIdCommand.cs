namespace Sale_Saas.Application.Features.RelationshipStatusFeature.Commands
{
    public record RelationshipStatus_DeleteByIdCommand(DeleteRequest RequestData) : IRequest<Result<string>>;
    public class RelationshipStatus_DeleteByIdCommandHandler : IRequestHandler<RelationshipStatus_DeleteByIdCommand, Result<string>>
    {

        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public RelationshipStatus_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(RelationshipStatus_DeleteByIdCommand request, CancellationToken cancellationToken)
        {
            string result = string.Empty;

            if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
            List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();
            var query = await _context.RelationshipStatuses.Where(m => ids.Contains(m.Id)).ToListAsync();
            if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");

            foreach (var item in query)
            {
                item.DeleteFlag = true;
            }

            _context.RelationshipStatuses.UpdateRange(query);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(string.Empty);
        }
    }
}
