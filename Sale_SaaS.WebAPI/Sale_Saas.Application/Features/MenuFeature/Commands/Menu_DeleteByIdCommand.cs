namespace Sale_Saas.Application.Features.MenuFeature.Commands;

public record Menu_DeleteByIdCommand(DeleteRequest RequestData) : IRequest<Result<string>>;
public class Menu_DeleteByIdCommandHandler : IRequestHandler<Menu_DeleteByIdCommand, Result<string>>
{
    
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper; 
	
	public Menu_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context)
    {        
		_context = context;
		_mapper = mapper;		
	}

    public async Task<Result<string>> Handle(Menu_DeleteByIdCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;

        if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");

        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

        var query = await _context.Menus.Where(m => ids.Contains(m.Id)).ToListAsync();
        if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");

        foreach (var item in query)
        {
            EventLog eventLog = new EventLog()
            {
                Code = "Menu",
                Name = "Menu",
                Action = "Xóa",
                Notes = "Id: " + item.Id.ToString() + ", Tên: " + item.Name,
                CreatedApplicationUserId = request.RequestData.ApplicationUserId,
                LastModifiedApplicationUserId = request.RequestData.ApplicationUserId
            };
            _context.EventLogs.Add(eventLog);
        }

        _context.Menus.RemoveRange(query);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(string.Empty);
    }
}
