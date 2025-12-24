using Sale_Saas.Application.Features.MenuFeature.Dto;
using Sale_Saas.Domain.Constants.API;

namespace Sale_Saas.Application.Features.MenuFeature.Commands;

/*public record Menu_InitFeatureCommand() : IRequest<Result<List<Menu>>>;
public class Menu_InitFeatureCommandHandler : IRequestHandler<Menu_InitFeatureCommand, Result<List<Menu>>>
{

	private readonly IApplicationDbContext _context;
	public Menu_InitFeatureCommandHandler(IApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<Result<List<Menu>>> Handle(Menu_InitFeatureCommand request, CancellationToken cancellationToken)
	{
		var menus = await _context.Menus.ToListAsync();
		if (_context.Menus.Any())
		{
			#region MODULE_SALE

			var features = await _context.Features.ToListAsync();
			var Sale_SK_PQ = menus.Where(s => s.Code == "Sale_SK_PQ").FirstOrDefault();
			if (Sale_SK_PQ != null)
			{
				var lst = FeatureDataConstant.sale_phanquyen_salekit_feature();
				Sale_SK_PQ.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}

			var Sale_SK = menus.Where(s => s.Code == "Sale_SK").FirstOrDefault();
			if (Sale_SK != null)
			{
				var lst = FeatureDataConstant.sale_salekit_feature();
				Sale_SK.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}

			var Sale_QL = menus.Where(s => s.Code == "Sale_QL").FirstOrDefault();
			if (Sale_QL != null)
			{
				var lst = FeatureDataConstant.sale_ql_feature();
				Sale_QL.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}

			var Sale_MT = menus.Where(s => s.Code == "Sale_MT").FirstOrDefault();
			if (Sale_MT != null)
			{
				var lst = FeatureDataConstant.sale_mt_feature();
				Sale_MT.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}

			var Sale_MQH = menus.Where(s => s.Code == "Sale_MQH").FirstOrDefault();
			if (Sale_MQH != null)
			{
				var lst = FeatureDataConstant.sale_mqh_feature();
				Sale_MQH.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}

			var Sale_CH = menus.Where(s => s.Code == "Sale_CH").FirstOrDefault();
			if (Sale_CH != null)
			{
				var lst = FeatureDataConstant.sale_cohoi_feature();
				Sale_CH.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}

			#endregion

			#region MODULE_NHAN_SU
			var lst_nhansu = new List<string>()
			{
				"NS_TTTC", "NS_TTNS", "NS_BCTN"
			};
			var NhanSus = menus.Where(s => s.Code != null && lst_nhansu.Contains(s.Code)).ToList();
			foreach (var item in NhanSus)
			{
				var lst = FeatureDataConstant.nhanhsu_feature();
				item.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}

			var ThuNhaps = menus.Where(s => s.Code == "NS_TTTN").FirstOrDefault();
			if (ThuNhaps != null)
			{
				var lst = FeatureDataConstant.thu_nhap_feature();
				ThuNhaps.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}
			#endregion

			#region MODULE_DANH_MUC
			var lst_danhmuc = new List<string>()
			{
				"DM_NCC", "DM_MDV", "DM_MDQH" , "DM_KH" , "DM_HD" , "DM_GAINS" , "DM_DA"
			};
			var DanhMucs = menus.Where(s => s.Code != null && lst_danhmuc.Contains(s.Code)).ToList();
			foreach (var item in DanhMucs)
			{
				var lst = FeatureDataConstant.common_feature();
				item.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}

			var DanhMucNhanSus = menus.Where(s => s.Code == "DM_NS").FirstOrDefault();
			if (DanhMucNhanSus != null)
			{
				var lst = FeatureDataConstant.danhmuc_nhansu_feature();
				DanhMucNhanSus.Features = features.Where(s => lst.Contains(s.Id)).ToList();
			}
			#endregion


			_context.Menus.UpdateRange(menus);
			await _context.SaveChangesAsync(cancellationToken);

		}

		return Result<List<Menu>>.Success(menus);
	}
}
*/