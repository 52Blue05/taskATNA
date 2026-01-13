using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Infrastructure.Authentication
{
	// Config Policy , policy sẽ là Menu code + Feature ID
	public static class PolicyTypes
	{
		#region Module_Danh_Muc
		public static class DM_HD
		{
			public const string VIEW = MenuType.DM_HD + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.DM_HD + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.DM_HD + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.DM_HD + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.DM_HD + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.DM_HD + "_" + FeatureType.IMPORT_EXCEL;
			public const string SEARCH = MenuType.DM_HD + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.DM_HD + "_" + FeatureType.UPDATE;
		}
		public static class DM_GAINS
		{
			public const string VIEW = MenuType.DM_GAINS + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.DM_GAINS + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.DM_GAINS + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.DM_GAINS + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.DM_GAINS + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.DM_GAINS + "_" + FeatureType.IMPORT_EXCEL;
			public const string SEARCH = MenuType.DM_GAINS + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.DM_GAINS + "_" + FeatureType.UPDATE;
		}
		public static class DM_DA
		{
			public const string VIEW = MenuType.DM_DA + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.DM_DA + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.DM_DA + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.DM_DA + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.DM_DA + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.DM_DA + "_" + FeatureType.IMPORT_EXCEL;
			public const string SEARCH = MenuType.DM_DA + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.DM_DA + "_" + FeatureType.UPDATE;
			public const string UPDATE_RESULT = MenuType.DM_DA + "_" + FeatureType.UPDATE_RESULT;
		}
		public static class DM_KH
		{
			public const string VIEW = MenuType.DM_KH + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.DM_KH + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.DM_KH + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.DM_KH + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.DM_KH + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.DM_KH + "_" + FeatureType.IMPORT_EXCEL;
			public const string SEARCH = MenuType.DM_KH + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.DM_KH + "_" + FeatureType.UPDATE;
		}
		public static class DM_MDQH
		{
			public const string VIEW = MenuType.DM_MDQH + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.DM_MDQH + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.DM_MDQH + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.DM_MDQH + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.DM_MDQH + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.DM_MDQH + "_" + FeatureType.IMPORT_EXCEL;
			public const string SEARCH = MenuType.DM_MDQH + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.DM_MDQH + "_" + FeatureType.UPDATE;
		}
		public static class DM_MDV
		{
			public const string VIEW = MenuType.DM_MDV + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.DM_MDV + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.DM_MDV + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.DM_MDV + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.DM_MDV + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.DM_MDV + "_" + FeatureType.IMPORT_EXCEL;
			public const string SEARCH = MenuType.DM_MDV + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.DM_MDV + "_" + FeatureType.UPDATE;
		}
		public static class DM_NCC
		{
			public const string VIEW = MenuType.DM_NCC + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.DM_NCC + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.DM_NCC + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.DM_NCC + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.DM_NCC + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.DM_NCC + "_" + FeatureType.IMPORT_EXCEL;
			public const string SEARCH = MenuType.DM_NCC + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.DM_NCC + "_" + FeatureType.UPDATE;
		}
		public static class DM_NS
		{
			public const string VIEW = MenuType.DM_NS + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.DM_NS + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.DM_NS + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.DM_NS + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.DM_NS + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.DM_NS + "_" + FeatureType.IMPORT_EXCEL;
			public const string SEARCH = MenuType.DM_NS + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.DM_NS + "_" + FeatureType.UPDATE;
		}
		#endregion

		#region Module_Sale
		public static class Sale_MT
		{
			public const string View = MenuType.Sale_MT + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.Sale_MT + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.Sale_MT + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.Sale_MT + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.Sale_MT + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.Sale_MT + "_" + FeatureType.IMPORT_EXCEL;
			public const string MT_CAPNHATDEXUAT = MenuType.Sale_MT + "_" + FeatureType.MT_CAPNHATDEXUAT;
			public const string MT_CHOT = MenuType.Sale_MT + "_" + FeatureType.MT_CHOT;
			public const string MT_CHOTDIEUCHINH = MenuType.Sale_MT + "_" + FeatureType.MT_CHOTDIEUCHINH;
			public const string MT_DEXUATCHINHSUA = MenuType.Sale_MT + "_" + FeatureType.MT_DEXUATCHINHSUA;
			public const string MT_DIEUCHINH = MenuType.Sale_MT + "_" + FeatureType.MT_DIEUCHINH;
			public const string MT_XEMDEXUAT = MenuType.Sale_MT + "_" + FeatureType.MT_XEMDEXUAT;
			public const string SEARCH = MenuType.Sale_MT + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.Sale_MT + "_" + FeatureType.UPDATE;
			public const string UPDATE_RESULT = MenuType.Sale_MT + "_" + FeatureType.UPDATE_RESULT;
			public const string EMPLOYEE = MenuType.Sale_MT + "_" + FeatureType.EMPLOYEE;
			public const string MANAGER = MenuType.Sale_MT + "_" + FeatureType.MANAGER;
			public const string MYSELF = MenuType.Sale_MT + "_" + FeatureType.MYSELF;
			public const string VIEW_REPORT = MenuType.Sale_MT + "_" + FeatureType.VIEW_REPORT;
		}
		public static class Sale_QL
		{
			public const string View = MenuType.Sale_QL + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.Sale_QL + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.Sale_QL + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.Sale_QL + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.Sale_QL + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.Sale_QL + "_" + FeatureType.IMPORT_EXCEL;
			public const string QL_CAPNHATDEXUAT = MenuType.Sale_QL + "_" + FeatureType.QL_CAPNHATDEXUAT;
			public const string QL_CHOT = MenuType.Sale_QL + "_" + FeatureType.QL_CHOT;
			public const string QL_CHOTDIEUCHINH = MenuType.Sale_QL + "_" + FeatureType.QL_CHOTDIEUCHINH;
			public const string QL_DEXUATCHINHSUA = MenuType.Sale_QL + "_" + FeatureType.QL_DEXUATCHINHSUA;
			public const string QL_XEMDEXUAT = MenuType.Sale_QL + "_" + FeatureType.QL_XEMDEXUAT;
			public const string SEARCH = MenuType.Sale_QL + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.Sale_QL + "_" + FeatureType.UPDATE;
			public const string UPDATE_RESULT = MenuType.Sale_QL + "_" + FeatureType.UPDATE_RESULT;
			public const string EMPLOYEE = MenuType.Sale_QL + "_" + FeatureType.EMPLOYEE;
			public const string MANAGER = MenuType.Sale_QL + "_" + FeatureType.MANAGER;
			public const string MYSELF = MenuType.Sale_QL + "_" + FeatureType.MYSELF;
			public const string VIEW_REPORT = MenuType.Sale_QL + "_" + FeatureType.VIEW_REPORT;
			public const string QL_HISTORY = MenuType.Sale_QL + "_" +FeatureType.QL_HISTORY;
		}

		public static class Sale_MQH
		{
			public const string View = MenuType.Sale_MQH + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.Sale_MQH + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.Sale_MQH + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.Sale_MQH + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.Sale_MQH + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.Sale_MQH + "_" + FeatureType.IMPORT_EXCEL;
			public const string MQH_CAPNHATBANGGAINS = MenuType.Sale_MQH + "_" + FeatureType.MQH_CAPNHATBANGGAINS;
			public const string MQH_CHOT = MenuType.Sale_MQH + "_" + FeatureType.MQH_CHOT;
			public const string MQH_DANHGIA = MenuType.Sale_MQH + "_" + FeatureType.MQH_DANHGIA;
			public const string SEARCH = MenuType.Sale_MQH + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.Sale_MQH + "_" + FeatureType.UPDATE;
			public const string UPDATE_RESULT = MenuType.Sale_MQH + "_" + FeatureType.UPDATE_RESULT;
			public const string EMPLOYEE = MenuType.Sale_MQH + "_" + FeatureType.EMPLOYEE;
			public const string MANAGER = MenuType.Sale_MQH + "_" + FeatureType.MANAGER;
			public const string MYSELF = MenuType.Sale_MQH + "_" + FeatureType.MYSELF;
			public const string VIEW_REPORT = MenuType.Sale_MQH + "_" + FeatureType.VIEW_REPORT;
		}

		public static class Sale_CH
		{
			public const string CH_DONGCOHOI = MenuType.Sale_CH + "_" + FeatureType.CH_DONGCOHOI;
			public const string CH_GANCOHOI = MenuType.Sale_CH + "_" + FeatureType.CH_GANCOHOI;
			public const string CH_THEMCAPNHAT = MenuType.Sale_CH + "_" + FeatureType.CH_THEMCAPNHAT;
			public const string CH_XEMCAPNHAT = MenuType.Sale_CH + "_" + FeatureType.CH_XEMCAPNHAT;

			public const string CREATE = MenuType.Sale_CH + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.Sale_CH + "_" + FeatureType.DELETE;
			public const string DETAIL = MenuType.Sale_CH + "_" + FeatureType.DETAIL;
			public const string EXPORT_EXCEL = MenuType.Sale_CH + "_" + FeatureType.EXPORT_EXCEL;
			public const string IMPORT_EXCEL = MenuType.Sale_CH + "_" + FeatureType.IMPORT_EXCEL;

			public const string SEARCH = MenuType.Sale_CH + "_" + FeatureType.SEARCH;
			public const string UPDATE = MenuType.Sale_CH + "_" + FeatureType.UPDATE;
			public const string View = MenuType.Sale_CH + "_" + FeatureType.VIEW;
			public const string MYSELF = MenuType.Sale_CH + "_" + FeatureType.MYSELF;
			public const string VIEW_REPORT = MenuType.Sale_CH + "_" + FeatureType.VIEW_REPORT;
		}

		public static class Sale_SK
		{
			public const string SEARCH = MenuType.Sale_SK + "_" + FeatureType.SEARCH;
			public const string VIEW = MenuType.Sale_SK + "_" + FeatureType.VIEW;
			public const string CREATE = MenuType.Sale_SK + "_" + FeatureType.CREATE;
			public const string DELETE = MenuType.Sale_SK + "_" + FeatureType.DELETE;
		}

		//public static class Sale_SK_PQ
		//{
		//	public const string SALEKIT_XEMPHANQUYEN = MenuType.Sale_SK_PQ + "_" + FeatureType.SALEKIT_XEMPHANQUYEN;
		//	public const string SALEKIT_CAPNHATPHANQUYEN = MenuType.Sale_SK_PQ + "_" + FeatureType.SALEKIT_CAPNHATPHANQUYEN;
		//}
		#endregion


	}
}
