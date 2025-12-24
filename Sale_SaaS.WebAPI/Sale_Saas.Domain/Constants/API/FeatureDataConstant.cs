using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Domain.Constants.API
{
    // ***** Truyền role = null đối với init data table FeatureMenu
    //		 và FeatureMenuRolePosition ( đối với những menu không thuộc module SALE )

    // ***** Truyền role = RolePositionEnum khi init data table DetailRole
    //       và FeatureMenuRolePosition ( chỉ với module SALE )
    public static class FeatureDataConstant
    {
        public static List<string> common_feature()
        {
            List<string> list = new List<string>()
            {
                FeatureType.VIEW,
                FeatureType.SEARCH,
                FeatureType.CREATE,
                FeatureType.UPDATE,
                FeatureType.DELETE,
                //FeatureType.DETAIL,
				//FeatureType.IMPORT_EXCEL,
				//FeatureType.EXPORT_EXCEL
			};

            return list;
        }

        public static List<string> sale_feature()
        {
            List<string> list = new List<string>()
            {
                FeatureType.MYSELF,
                FeatureType.EMPLOYEE,
                FeatureType.MANAGER,
                FeatureType.UPDATE_RESULT,
                FeatureType.VIEW_REPORT
            };

            return list;
        }

        #region MODULE_SALE

        public static List<string> sale_mt_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.MANAGER)
            {
                List<string> list =
                [
                    FeatureType.MT_CHOT,
                    FeatureType.MT_DIEUCHINH,
                    FeatureType.MT_XEMDEXUAT,
                    FeatureType.MT_CAPNHATDEXUAT,
					// ***********************
					FeatureType.MYSELF,
                    FeatureType.EMPLOYEE,
                    FeatureType.VIEW_REPORT,
					// ***********************
					FeatureType.CREATE,FeatureType.UPDATE,
                    FeatureType.DELETE,
					//FeatureType.DETAIL,
					//FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL
				];
                return list;
            }
            else if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list =
                [
                    FeatureType.MT_CHOTDIEUCHINH,
                    //FeatureType.MT_XEMDEXUAT,
                    FeatureType.MT_DEXUATCHINHSUA,
					// ***********************
					FeatureType.MYSELF,
                    FeatureType.VIEW_REPORT,
					// ***********************
					FeatureType.CREATE,FeatureType.UPDATE,
                    FeatureType.DELETE,
					//FeatureType.DETAIL
				];
                return list;
            }
            else if (role == RolePositionEnum.ADMINISTRATOR)
            {
                List<string> list =
                [
                    FeatureType.UPDATE_RESULT,
                    FeatureType.EMPLOYEE,
                    FeatureType.MANAGER,
                    FeatureType.VIEW_REPORT,
					//FeatureType.DETAIL,
					//FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL
				];
                return list;
            }
            else
            {
                List<string> list =
                [
                    FeatureType.MT_CHOT,
                    FeatureType.MT_DIEUCHINH,
                    FeatureType.MT_CHOTDIEUCHINH,
                    FeatureType.MT_DEXUATCHINHSUA,
                    FeatureType.MT_XEMDEXUAT,
                    FeatureType.MT_CAPNHATDEXUAT,
                    .. sale_feature(),
					//.. common_feature()
					FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.DELETE,
					//FeatureType.DETAIL,
					//FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL
				];
                return list;
            }

        }

        public static List<string> sale_ql_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.MANAGER)
            {
                List<string> list =
                [
                    FeatureType.QL_CHOT,
                    FeatureType.QL_XEMDEXUAT,
                    //FeatureType.QL_CAPNHATDEXUAT,
                    FeatureType.QL_THUAHUONG,
                    FeatureType.QL_HISTORY,
					// **********************************
					FeatureType.MYSELF,
                    FeatureType.EMPLOYEE,
                    FeatureType.VIEW_REPORT,
					// **********************************
					FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.DELETE,
                    //FeatureType.DETAIL
                ];
                return list;
            }
            if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list =
                [
                    FeatureType.QL_CHOT,
                    //FeatureType.QL_CHOTDIEUCHINH,
                    //FeatureType.QL_XEMDEXUAT,
                    FeatureType.QL_DEXUATCHINHSUA,
                    FeatureType.QL_THUAHUONG,
					// **********************************
					FeatureType.MYSELF,
                    FeatureType.VIEW_REPORT,
					// **********************************
					//FeatureType.DETAIL
                     FeatureType.QL_HISTORY,
                ];
                return list;
            }
            if (role == RolePositionEnum.ADMINISTRATOR)
            {
                List<string> list =
                [
                    FeatureType.MANAGER,
                    FeatureType.EMPLOYEE,
                    FeatureType.VIEW_REPORT,
                    FeatureType.UPDATE_RESULT,
					// **********************************
					FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.DELETE,
                    //FeatureType.DETAIL
                     FeatureType.QL_HISTORY,

                ];
                return list;
            }
            else
            {
                List<string> list =
                [
                    FeatureType.QL_CHOT,
                    FeatureType.QL_CHOTDIEUCHINH,
                    FeatureType.QL_DEXUATCHINHSUA,
                    FeatureType.QL_XEMDEXUAT,
                    FeatureType.QL_CAPNHATDEXUAT,
                    FeatureType.QL_THUAHUONG,
                    FeatureType.QL_HISTORY,
                    .. sale_feature(),
					//.. common_feature()
					FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.DELETE,
                    //FeatureType.DETAIL,
                    FeatureType.IMPORT_EXCEL,
                    FeatureType.EXPORT_EXCEL,
                ];
                return list;
            }

        }

        public static List<string> sale_mqh_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.MANAGER)
            {
                List<string> list =
                [
                    FeatureType.MQH_CHOT,
                    FeatureType.MQH_CAPNHATBANGGAINS,
                    FeatureType.MQH_DANHGIA,
					// ********************************
					FeatureType.MYSELF,
                    FeatureType.EMPLOYEE,
                    FeatureType.VIEW_REPORT,
					// ********************************
					FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.DELETE,
                    //FeatureType.DETAIL
                ];
                return list;
            }
            if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list =
                [
                    FeatureType.MQH_CAPNHATBANGGAINS,
                    FeatureType.MQH_DANHGIA,
					// ********************************
					FeatureType.MYSELF,
                    FeatureType.VIEW_REPORT,
					// ********************************
					FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.DELETE,
                    //FeatureType.DETAIL
                ];
                return list;
            }
            if (role == RolePositionEnum.ADMINISTRATOR)
            {
                List<string> list =
                [
                    FeatureType.MANAGER,
                    FeatureType.EMPLOYEE,
                    FeatureType.VIEW_REPORT,
                    FeatureType.UPDATE_RESULT,
					// ********************************
					//FeatureType.DETAIL
                ];
                return list;
            }
            else
            {
                List<string> list =
                [
                    FeatureType.MQH_CHOT,
                    FeatureType.MQH_CAPNHATBANGGAINS,
                    FeatureType.MQH_DANHGIA,
                    FeatureType.UPDATE_RESULT,
                    FeatureType.MYSELF,
                    FeatureType.MANAGER,
                    FeatureType.EMPLOYEE,
                    FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.DELETE,
                    FeatureType.DETAIL,
                    FeatureType.VIEW_REPORT,
                    FeatureType.IMPORT_EXCEL,
                    FeatureType.EXPORT_EXCEL,
                ];
                return list;
            }

        }

        public static List<string> sale_cohoi_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.MANAGER)
            {
                List<string> list =
                [
                    FeatureType.CH_GANCOHOI,
                    FeatureType.CH_XEMCAPNHAT,
                    FeatureType.CH_THEMCAPNHAT,
                    FeatureType.CH_DONGCOHOI,
                    //FeatureType.MYSELF,
                    FeatureType.VIEW_REPORT,
					//.. common_feature()
					FeatureType.VIEW,
                    FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.DELETE,
                    //FeatureType.DETAIL,
                    //FeatureType.IMPORT_EXCEL,
                    //FeatureType.EXPORT_EXCEL
                ];

                return list;
            }
            else if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list =
                [
                    FeatureType.CH_XEMCAPNHAT,
                    FeatureType.CH_THEMCAPNHAT,
                    FeatureType.CH_DONGCOHOI,
                    FeatureType.MYSELF,
                    FeatureType.VIEW_REPORT,
					//.. common_feature()
					FeatureType.CREATE,
                    FeatureType.UPDATE,
                    //FeatureType.DETAIL
                ];

                return list;
            }
            else
            {
                List<string> list =
                [
                    FeatureType.CH_GANCOHOI,
                    FeatureType.CH_XEMCAPNHAT,
                    FeatureType.CH_THEMCAPNHAT,
                    FeatureType.CH_DONGCOHOI,
                    FeatureType.MYSELF,
                    FeatureType.VIEW_REPORT,
					//.. common_feature()
					FeatureType.VIEW,
                    FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.DELETE,
                    FeatureType.DETAIL,
                    FeatureType.IMPORT_EXCEL,
                    FeatureType.EXPORT_EXCEL
                ];

                return list;
            }

        }

        public static List<string> sale_salekit_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.ADMINISTRATOR || role == RolePositionEnum.MANAGER)
            {
                List<string> list =
                [
                    FeatureType.VIEW,
                    FeatureType.CREATE,
                    FeatureType.DELETE,
                    FeatureType.SALEKIT_XEMPHANQUYEN,
                    FeatureType.SALEKIT_CAPNHATPHANQUYEN
                ];
                return list;
            }
            if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list =
                [
                    FeatureType.VIEW
                ];
                return list;
            }
            else
            {
                List<string> list =
                [
                    FeatureType.VIEW,
                    FeatureType.CREATE,
                    FeatureType.DELETE,
                    FeatureType.SALEKIT_XEMPHANQUYEN,
                    FeatureType.SALEKIT_CAPNHATPHANQUYEN
                ];
                return list;
            }
        }

        public static List<string> sale_elearning_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.MANAGER)
            {
                List<string> list =
                [
                    FeatureType.VIEW,
                    FeatureType.EL_GD_TAOCHUONGTRINH,
                    FeatureType.EL_GD_CAPNHATCHUONGTRINH,
                    FeatureType.EL_GD_XOACHUONGTRINH,
                    FeatureType.EL_GD_THEMKHOA,
                    FeatureType.EL_GD_XOAKHOA,
                    FeatureType.EL_GD_THEMHOCVIEN,
                    FeatureType.EL_GD_XOAHOCVIEN,
                ];
                return list;
            }
            if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list =
                [
                    FeatureType.VIEW,
                    FeatureType.EL_NV_XEMNOIDUNG,
                    FeatureType.EL_NV_LAMBAIKIEMTRA
                ];
                return list;
            }
            else
            {
                List<string> list =
                [
                    FeatureType.VIEW,
                    FeatureType.EL_NCN_TAOKHOA,
                    FeatureType.EL_NCN_SUAKHOA,
                    FeatureType.EL_NCN_XOAKHOA,
                    FeatureType.EL_NCN_TAOBAIKIEMTRA,
                ];
                return list;
            }
        }

        //public static List<string> sale_phanquyen_salekit_feature(RolePositionEnum? role = null)
        //{
        //	List<string> list =
        //	[
        //		FeatureType.SALEKIT_XEMPHANQUYEN,
        //		FeatureType.SALEKIT_CAPNHATPHANQUYEN
        //	];

        //	return list;
        //}

        #endregion

        #region MODULE_NHAN_SU

        public static List<string> nhanhsu_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.ADMINISTRATOR || role == RolePositionEnum.MANAGER)
            {
                List<string> list =
                [
					//FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL,
					FeatureType.VIEW,
                ];
                return list;
            }
            else if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list =
                [
					//FeatureType.MYSELF,
                    FeatureType.VIEW,
                ];
                return list;
            }
            else
            {
                List<string> list =
                [
					//FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL,
					FeatureType.VIEW,
					//FeatureType.MYSELF,
				];
                return list;
            }

        }

        public static List<string> thu_nhap_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.ADMINISTRATOR || role == RolePositionEnum.MANAGER)
            {
                List<string> list =
                [
                    FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL,
					FeatureType.TTTN_HIENTAI_ALL,
                    FeatureType.TTTN_VITRI_ALL,
                    FeatureType.TTTN_DETAIL
                ];
                return list;
            }
            else if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list =
                [
                    FeatureType.TTTN_HIENTAI_MYSELF,
                    FeatureType.TTTN_VITRI_MYSELF,
                    FeatureType.TTTN_DETAIL
                ];
                return list;
            }
            else
            {
                List<string> list =
                [
                    FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL,
					FeatureType.TTTN_HIENTAI_ALL,
                    FeatureType.TTTN_VITRI_ALL,
                    FeatureType.TTTN_DETAIL,
                    FeatureType.TTTN_HIENTAI_MYSELF,
                    FeatureType.TTTN_VITRI_MYSELF,
                ];
                return list;
            }

        }

        #endregion

        #region MODULE_DANH_MUC

        public static List<string> danhmuc_nhansu_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.ADMINISTRATOR || role == RolePositionEnum.MANAGER)
            {
                List<string> list = new List<string>()
                {
                    FeatureType.VIEW,
                    FeatureType.SEARCH,
                    FeatureType.UPDATE,
					//FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL
				};

                return list;
            }
            else if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list =
                [
                    FeatureType.SEARCH,
                ];
                return list;
            }
            else
            {
                List<string> list = new List<string>()
                {
                    FeatureType.VIEW,
                    FeatureType.SEARCH,
                    FeatureType.UPDATE,
					//FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL
				};

                return list;
            }
        }

        public static List<string> danhmuc_du_an_feature(RolePositionEnum? role = null)
        {
            if (role == RolePositionEnum.EMPLOYEE)
            {
                List<string> list = new List<string>()
                {
                    FeatureType.SEARCH
                };

                return list;
            }
            else
            {
                List<string> list = new List<string>()
                {
                    FeatureType.VIEW,
                    FeatureType.SEARCH,
                    FeatureType.CREATE,
                    FeatureType.UPDATE,
                    FeatureType.UPDATE_RESULT,
                    FeatureType.DELETE,
					//FeatureType.DETAIL,
					//FeatureType.IMPORT_EXCEL,
					//FeatureType.EXPORT_EXCEL
				};

                return list;
            }
        }

        #endregion

    }
}
