using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.MenuFeature.Dto;


namespace Sale_Saas.Application.Features.MenuFeature.Queries
{    public record Menu_GetByMaQuery: IRequest<Result<IEnumerable<MenuDto>>>
    {
        public string Code { get; init; }
    }
    public class Menu_GetByMaQueryHandler : IRequestHandler<Menu_GetByMaQuery, Result<IEnumerable<MenuDto>>>
    {        
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Menu_GetByMaQueryHandler(IMapper mapper, IApplicationDbContext context)
        {            
            _context = context;
            _mapper = mapper;
        }
        public async Task<Result<IEnumerable<MenuDto>>> Handle(Menu_GetByMaQuery request, CancellationToken cancellationToken)
        {
            return Result<IEnumerable<MenuDto>>.Success((await (from m in _context.Menus
                                                                where m.DeleteFlag != true
                                                                    && m.Code == request.Code
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
                                                                    Link = m.Link ?? string.Empty
                                                                })
                                                            .AsNoTracking()
                                                            .ToListAsync())
                                                            .AsReadOnly());
        }
	}
}
