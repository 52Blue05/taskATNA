namespace Sale_Saas.Application.Features.RolePositionFeature.Commands
{
	public record RolePosition_DeleteByIdCommand(DeleteRequest RequestData) : IRequest<Result<string>>;
	public class RolePosition_DeleteByIdCommandHandler : IRequestHandler<RolePosition_DeleteByIdCommand, Result<string>>
	{

		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
		public RolePosition_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<Result<string>> Handle(RolePosition_DeleteByIdCommand request, CancellationToken cancellationToken)
		{
			string result = string.Empty;

			if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
			List<string> ids = request.RequestData.Ids.ToList();
			var query = await _context.RolePositions.Where(m => ids.Contains(m.Id)).ToListAsync();
			if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");

			foreach (var item in query)
			{
				item.DeleteFlag = true;
				item.LastModifiedDate = DateTime.Now;
				item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
			}

			_context.RolePositions.UpdateRange(query);

			await _context.SaveChangesAsync(cancellationToken);

			return Result<string>.Success(string.Empty);
		}
	}
}
