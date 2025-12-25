using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.MenuFeature.Dto;



namespace Sale_Saas.Application.Features.MenuFeature.Queries
{
    public record Menu_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<MenuDto>>>;
    public class Menu_GetListWithPaginationQueryHandler : IRequestHandler<Menu_GetListWithPaginationQuery, Result<PaginatedList<MenuDto>>>
    {        
		private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Menu_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context)
        {            
			_context = context;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<MenuDto>>> Handle(Menu_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            return Result<PaginatedList<MenuDto>>.Success(await _context.Menus.Where(m=>m.IsActivite==true && m.DeleteFlag!=true)
                                    .OrderBy(x => x.Code)
                                    .ProjectTo<MenuDto>(_mapper.ConfigurationProvider)
                                    .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
	}
}
