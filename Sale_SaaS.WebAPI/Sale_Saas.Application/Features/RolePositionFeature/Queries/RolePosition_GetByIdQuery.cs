using Sale_Saas.Application.Features.RolePositionFeature.Dto;

namespace Sale_Saas.Application.Features.RolePositionFeature.Queries
{
	public record RolePosition_GetByIdQuery(string Id) : IRequest<Result<RolePositionDto>>;

	public class RolePosition_GetByIdQueryHandler : IRequestHandler<RolePosition_GetByIdQuery, Result<RolePositionDto>>
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;

		public RolePosition_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<Result<RolePositionDto>> Handle(RolePosition_GetByIdQuery request, CancellationToken cancellationToken)
		{
			RolePositionDto? RolePosition = await (from cv in _context.RolePositions
												   where cv.DeleteFlag != true && cv.Id == request.Id && cv.IsModified != false
												   select new RolePositionDto()
												   {
													   Id = cv.Id,
													   Name = cv.Name ?? "",
													   Level = cv.Level
												   }).AsNoTracking().FirstOrDefaultAsync();
			return Result<RolePositionDto>.Success(RolePosition);
		}
	}
}
