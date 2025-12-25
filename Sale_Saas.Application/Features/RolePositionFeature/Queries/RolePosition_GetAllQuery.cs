using Sale_Saas.Application.Features.RolePositionFeature.Dto;

namespace Sale_Saas.Application.Features.RolePositionFeature.Queries
{
	public record RolePosition_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<RolePositionDto>>>;

	public class RolePosition_GetAllQueryHandler : IRequestHandler<RolePosition_GetAllQuery, Result<IEnumerable<RolePositionDto>>>
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;

		public RolePosition_GetAllQueryHandler(IMapper mapper,
										IApplicationDbContext context)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<Result<IEnumerable<RolePositionDto>>> Handle(RolePosition_GetAllQuery request, CancellationToken cancellationToken)
		{
			var query = from cv in _context.RolePositions
						where cv.DeleteFlag != true && cv.IsModified != false
						select new RolePositionDto()
						{
							Id = cv.Id,
							Name = cv.Name ?? "",
							Level = cv.Level
						};

			if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
			{
				string search = request.RequestData.TextSearch.Trim().ToLower();
				query = query.Where(cv => cv.Name.ToLower().Contains(search) ||
										  cv.Id.ToLower().Contains(search));
			}

			var RolePositions = await query.AsNoTracking().ToListAsync();
			return Result<IEnumerable<RolePositionDto>>.Success(RolePositions.AsReadOnly());
		}
	}
}
