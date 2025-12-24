using Sale_Saas.Application.Features.MenuFeature.Dto;
using Sale_Saas.Application.Features.MenuFeature.Dto;
using System.Drawing;
using System.Reflection.Metadata;


namespace Sale_Saas.Application.Features.MenuFeature.Queries
{    public record Menu_GetListQuery(FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<MenuDto>>>;
    public class Menu_GetListQueryHandler : IRequestHandler<Menu_GetListQuery, Result<IEnumerable<MenuDto>>>
    {
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;

		public Menu_GetListQueryHandler(IMapper mapper, IApplicationDbContext context)		
		{			
			_context = context;
			_mapper = mapper;
		}
        public async Task<Result<IEnumerable<MenuDto>>> Handle(Menu_GetListQuery request, CancellationToken cancellationToken)
        {
			var query = from m in _context.Menus
						where m.DeleteFlag != true && m.IsActivite==true
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
						};

			if (request.RequestData.Skip != null)
			{
				query = query.Skip(request.RequestData.Skip.Value);
			}

			if (request.RequestData.TotalRecord != null)
			{
				query = query.Take(request.RequestData.TotalRecord.Value);
			}

			return Result<IEnumerable<MenuDto>>.Success((await query.AsNoTracking()
																	.ToListAsync())
																	.AsReadOnly());
        }
	}
}
