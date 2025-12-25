
namespace Sale_Saas.Application.Features.MenuFeature.Commands;

public record Menu_ImportingDataCommand(ImportingDataRequest RequestData) : IRequest<Result<string>>;
public class Menu_ImportingDataCommandHandler : IRequestHandler<Menu_ImportingDataCommand, Result<string>>
{
    
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper; 
	
	public Menu_ImportingDataCommandHandler(IMapper mapper, IApplicationDbContext context)
    {        
		_context = context;
		_mapper = mapper;		
	}

    public async Task<Result<string>> Handle(Menu_ImportingDataCommand request, CancellationToken cancellationToken)
    {
        string resultMessage = String.Empty;
        int sttValue = 0;
        Guid guidValue;

        if (request.RequestData.Dt.Rows.Count > 0)
        {
            EventLog eventLog = new EventLog()
            {
                Code = "Menu",
                Name = "Menu",
                Action = "Import data",
                CreatedApplicationUserId = request.RequestData.CreatedApplicationUserId,
                LastModifiedApplicationUserId = request.RequestData.CreatedApplicationUserId
            };
            _context.EventLogs.Add(eventLog);
            await _context.SaveChangesAsync(cancellationToken);

            int rowIndex = 0;
            while (rowIndex < request.RequestData.Dt.Rows.Count)
            {
                string updateStatus = request.RequestData.Dt.Rows[rowIndex][0].ToString() ?? string.Empty;
                string code = request.RequestData.Dt.Rows[rowIndex][1].ToString() ?? string.Empty;
                string name = request.RequestData.Dt.Rows[rowIndex][2].ToString() ?? string.Empty;
                string sortOrder = request.RequestData.Dt.Rows[rowIndex][3].ToString() ?? string.Empty;
                string parentId = request.RequestData.Dt.Rows[rowIndex][4].ToString() ?? string.Empty;
                string nameController = request.RequestData.Dt.Rows[rowIndex][5].ToString() ?? string.Empty;
                string nameAction = request.RequestData.Dt.Rows[rowIndex][6].ToString() ?? string.Empty;
                string parameter = request.RequestData.Dt.Rows[rowIndex][7].ToString() ?? string.Empty;
                string icon = request.RequestData.Dt.Rows[rowIndex][8].ToString() ?? string.Empty;
                string link = request.RequestData.Dt.Rows[rowIndex][9].ToString() ?? string.Empty;

                if (updateStatus.ToLower() != "y")
                {
                    if (!String.IsNullOrEmpty(code) && !String.IsNullOrEmpty(name))
                    {
                        Menu? menu = await _context.Menus.FirstOrDefaultAsync(m => !string.IsNullOrEmpty(m.Code)
                                                                    && m.Code.Trim().ToLower().Equals(code.Trim().ToLower()));
                        if (menu == null)
                        {
                            await _context.Menus.AddAsync(new Menu()
                            {
                                Code = code.Trim(),
                                Name = name.Trim(),
                                SortOrder = Int32.TryParse(sortOrder, out sttValue) ? sttValue : 0,
                                NameAction = !string.IsNullOrEmpty(nameAction) ? nameAction.Trim() : string.Empty,
                                NameController = !string.IsNullOrEmpty(nameController) ? nameController.Trim() : string.Empty,
                                Parameter = !string.IsNullOrEmpty(parameter) ? parameter.Trim() : string.Empty,
                                ParentId = Guid.TryParse(parentId, out guidValue) ? guidValue : null,
                                Icon = !string.IsNullOrEmpty(icon) ? icon.Trim() : string.Empty,
                                Link = !string.IsNullOrEmpty(link) ? link.Trim() : string.Empty,
                                CreatedApplicationUserId = request.RequestData.CreatedApplicationUserId,
                                LastModifiedApplicationUserId = request.RequestData.CreatedApplicationUserId
                            });
                        }
                        else
                        {
                            menu.Code = code.Trim();
                            menu.Name = name.Trim();
                            menu.SortOrder = Int32.TryParse(sortOrder, out sttValue) ? sttValue : 0;
                            menu.NameAction = !string.IsNullOrEmpty(nameAction) ? nameAction.Trim() : string.Empty;
                            menu.NameController = !string.IsNullOrEmpty(nameController) ? nameController.Trim() : string.Empty;
                            menu.Parameter = !string.IsNullOrEmpty(parameter) ? parameter.Trim() : string.Empty;
                            menu.ParentId = Guid.TryParse(parentId, out guidValue) ? guidValue : null;
                            menu.Icon = !string.IsNullOrEmpty(icon) ? icon.Trim() : string.Empty;
                            menu.Link = !string.IsNullOrEmpty(link) ? link.Trim() : string.Empty;
                            menu.LastModifiedApplicationUserId = request.RequestData.CreatedApplicationUserId;
                            _context.Menus.Update(menu);
                        }                        
                    }
                }

                rowIndex++;
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<string>.Success(resultMessage);
    }
}
