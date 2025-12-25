using Sale_Saas.Application.Features.FeaturePermissionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.FeaturePermissionFeature.Queries;

public record FeaturePermission_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<List<MenuWithFeatureDto>>>;
public class FeaturePermission_GetAllQueryHandler : IRequestHandler<FeaturePermission_GetAllQuery, Result<List<MenuWithFeatureDto>>>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

    public FeaturePermission_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
	{
		_context = context;
		_mapper = mapper;
		_eventLogService = eventLogService;
	}

	public async Task<Result<List<MenuWithFeatureDto>>> Handle(FeaturePermission_GetAllQuery request, CancellationToken cancellationToken)
	{
		var featureMenus = await (from fm in _context.FeatureMenus
								  join menu in _context.Menus on fm.MenuId equals menu.Id
								  join feature in _context.Features on fm.FeatureId equals feature.Id
								  where menu.DeleteFlag != true && feature.DeleteFlag != true
								  select new
								  {
									  Menu = menu,
									  Feature = feature
								  }).AsNoTracking().ToListAsync();

		var groupedMenus = featureMenus.GroupBy(fm => fm.Menu.Id)
									   .Select(g => new MenuWithFeatureDto
									   {
										   Id = g.Key,
										   Name = g.First().Menu.Name,
										   Code = g.First().Menu.Code,
										   ParentId = g.First().Menu.ParentId,
										   SortOrder = g.First().Menu.SortOrder,
										   Features = g.Select(fm => new FeaturePermissionDto
										   {
											   Id = fm.Feature.Id,
											   Name = fm.Feature.Name,
											   Access = false
										   }).ToList()
									   }).OrderByDescending(s => s.Code).ToList();

        var eventLog = await _eventLogService.Create("FeaturePermissionFeature", "FeaturePermissionFeature",
                                                "FeaturePermission_GetAllQuery", request.userId);

        return Result<List<MenuWithFeatureDto>>.Success(groupedMenus);
	}

	private void AddMenuWithChildren(List<MenuWithFeatureDto> sortedList, MenuWithFeatureDto menu, Dictionary<Guid, MenuWithFeatureDto> menuDictionary)
	{
		sortedList.Add(menu);

		var childMenus = menuDictionary.Values.Where(m => m.ParentId == menu.Id).OrderBy(m => m.SortOrder).ToList();

		foreach (var childMenu in childMenus)
		{
			AddMenuWithChildren(sortedList, childMenu, menuDictionary);
		}
	}


}
