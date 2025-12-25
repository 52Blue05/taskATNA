namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Commands
{
    public record RelationshipLevel_DeleteByIdCommand(DeleteRequest RequestData) : IRequest<Result<string>>;
    public class RelationshipLevel_DeleteByIdCommandHandler : IRequestHandler<RelationshipLevel_DeleteByIdCommand, Result<string>>
    {

        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public RelationshipLevel_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(RelationshipLevel_DeleteByIdCommand request, CancellationToken cancellationToken)
        {
            string result = string.Empty;

            if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
            List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();
            var query = await _context.RelationshipLevels.Where(m => ids.Contains(m.Id)).ToListAsync();
            if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");

            foreach (var item in query)
            {
                item.DeleteFlag = true;
                item.SortOrder = null;
                item.LastModifiedDate = DateTime.Now;
                item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
            }

            _context.RelationshipLevels.UpdateRange(query);

            await _context.SaveChangesAsync(cancellationToken);

            // handle re-sort
            var listRelationshipLevel = await _context.RelationshipLevels.Where(x => x.DeleteFlag != true)
                                                                         .OrderBy(x => x.SortOrder)
                                                                         .ToListAsync();

            const int INITIAL_SORT_ORDER = 1;
            var index = INITIAL_SORT_ORDER;

            foreach (var item in listRelationshipLevel)
            {
                item.SortOrder = index;
                index = index + 1;
            }

            _context.RelationshipLevels.UpdateRange(listRelationshipLevel);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(string.Empty);
        }
    }
}
