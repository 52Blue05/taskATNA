using Sale_Saas.Application.Features.MenuFeature.Dto;


namespace Sale_Saas.Application.Features.MenuFeature.Queries
{
    public record Menu_GetByIdQuery(Guid Id) : IRequest<Result<MenuDto>>;
    public class Menu_GetByIdQueryHandler : IRequestHandler<Menu_GetByIdQuery, Result<MenuDto>>
    {        
        private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
		public Menu_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {			
			_context = context;
			_mapper = mapper;
		}

        public async Task<Result<MenuDto>> Handle(Menu_GetByIdQuery request, CancellationToken cancellationToken)
        {
            return Result<MenuDto>.Success(await (from m in _context.Menus
                                                   where m.DeleteFlag != true && m.Id == request.Id
                                                   select new MenuDto()
                                                   {
                                                       Id = m.Id,
                                                       Code = m.Code ?? string.Empty,
                                                       Name = m.Name ?? string.Empty,
                                                       SortOder = m.SortOrder,
                                                       NameAction = m.NameAction ?? string.Empty,
                                                       NameController = m.NameController ?? string.Empty,
                                                       Parameter = m.Parameter ?? string.Empty,
                                                       ParentId = m.ParentId,
                                                       Icon = m.Icon ?? string.Empty,
                                                       Link = m.Link
                                                   })
                          .AsNoTracking()
                          .FirstOrDefaultAsync());
        }
	}
}
