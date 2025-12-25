using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.SaleKitFeature.Dto;

namespace Sale_Saas.Application.Features.SaleKitFeature.Queries;

public record SaleKit_GetParentQuery(Guid Id) : IRequest<Result<SalekitParentDto>>;

public class SaleKit_GetParentQueryHandler : IRequestHandler<SaleKit_GetParentQuery, Result<SalekitParentDto>>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;

	public SaleKit_GetParentQueryHandler(IMapper mapper, IApplicationDbContext context)
	{
		_context = context;
		_mapper = mapper;
	}

	public async Task<Result<SalekitParentDto>> Handle(SaleKit_GetParentQuery request, CancellationToken cancellationToken)
	{
		var datas = await GetParent(request.Id);
		return Result<SalekitParentDto>.Success(datas);
	}

	private async Task<SalekitParentDto> GetParent(Guid? id)
	{
		if (id == null || id == Guid.Empty)
		{
			return null;
		}

		var salekit = await _context.SaleKits.Where(s => s.Id == id && s.DeleteFlag != true).FirstOrDefaultAsync();
		if (salekit == null)
		{
			return null;
		}

		var data = new SalekitParentDto
		{
			Id = salekit.Id,
			Name = salekit.Name ?? string.Empty,
			Description = salekit.Description ?? string.Empty,
			Parent = await GetParent(salekit.ParentId)
		};

		return data;
	}
}
