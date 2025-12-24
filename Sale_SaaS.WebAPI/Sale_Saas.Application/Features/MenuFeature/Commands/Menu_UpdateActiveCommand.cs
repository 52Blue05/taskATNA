using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.MenuFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.MenuFeature.Commands;

public record Menu_UpdateActiveCommand(MenuUpdateActiveDto RequestData) : IRequest<Result<List<MenuDto>>>;
public class Menu_UpdateActiveCommandHandler : IRequestHandler<Menu_UpdateActiveCommand, Result<List<MenuDto>>>
{
    
	private readonly IApplicationDbContext _context;
    public Menu_UpdateActiveCommandHandler(IApplicationDbContext context)
    {        
		_context = context;
    }

    public async Task<Result<List<MenuDto>>> Handle(Menu_UpdateActiveCommand request, CancellationToken cancellationToken)
    {
        List<MenuDto> updatedSuccess = new List<MenuDto>();
        if(!string.IsNullOrEmpty(request.RequestData.ConnectString))
        {
            _context.SetConnectString( request.RequestData.ConnectString);
        }
        foreach (string item in request.RequestData.Modules)
        {
            var listMenu=await _context.Menus.Where(x=>x.Code.Contains(item)).ToListAsync();

            if (!listMenu.Any())
            {
                throw new ApplicationException($"Không có dữ liệu.");
            }
            listMenu.ForEach(x =>
            {
                x.IsActivite = true;
                x.LastModifiedApplicationUserId = request.RequestData.LastModifiedApplicationUserId;
                x.LastModifiedDate = DateTime.Now;
            }
            ) ;
            updatedSuccess.Add(new MenuDto() { Id = listMenu[0].Id });
        }
        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<MenuDto>>.Success(updatedSuccess);
    }
}
