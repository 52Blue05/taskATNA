namespace Sale_Saas.Domain.Enums
{
	public enum FeatureEnum
	{
		// ********************** COMMON FEATURE
		VIEW, SEARCH, CREATE, UPDATE, DELETE, DETAIL, IMPORT_EXCEL, EXPORT_EXCEL,
		//VIEW, SEARCH, CREATE, UPDATE, DELETE, IMPORT_EXCEL, EXPORT_EXCEL,

        // **********************  SALE FEATURE
        VIEW_MYSELF, VIEW_EMPLOYEES, VIEW_MANAGER, UPDATE_RESULT, VIEW_REPORT,
		// => MỤC TIÊU
		MT_DIEUCHINH, MT_CHOTDIEUCHINH, MT_CHOT, MT_DEXUATCHINHSUA, MT_XEMDEXUAT, MT_CAPNHATDEXUAT,
		// => QUYỀN LỢI
		QL_CHOT, QL_CHOTDIEUCHINH, QL_DEXUATCHINHSUA, QL_XEMDEXUAT, QL_CAPNHATDEXUAT, QL_HISTORY,
        // => MỐI QUAN HỆ
        MQH_CHOT, MQH_CAPNHATBANGGAINS, MQH_DANHGIA,
		// => CƠ HỘI
		CH_GANCOHOI, CH_XEMCAPNHAT, CH_THEMCAPNHAT, CH_DONGCOHOI,
		// => SALEKIT
		SALEKIT_XEMPHANQUYEN, SALEKIT_CAPNHATPHANQUYEN,
        // => TỰ HỌC
        EL_NV_XEMNOIDUNG, EL_NV_LAMBAIKIEMTRA,
        // =>QUẢN LÝ TỰ HỌC
        EL_GD_TAOCHUONGTRINH, EL_GD_CAPNHATCHUONGTRINH, EL_GD_XOACHUONGTRINH, EL_GD_THEMKHOA, EL_GD_XOAKHOA, EL_GD_THEMHOCVIEN, EL_GD_XOAHOCVIEN,
        // => QUẢN LÝ KHÓA HOC
        EL_NCN_TAOKHOA, EL_NCN_SUAKHOA, EL_NCN_XOAKHOA, EL_NCN_TAOBAIKIEMTRA
}

	public static class FeatureType
	{
		// ********************** COMMON FEATURE
		public const string VIEW = "VIEW";
		public const string SEARCH = "SEARCH";
		public const string CREATE = "CREATE";
		public const string UPDATE = "UPDATE";
		public const string DELETE = "DELETE";
		public const string DETAIL = "DETAIL";
		public const string IMPORT_EXCEL = "IMPORT_EXCEL";
		public const string EXPORT_EXCEL = "EXPORT_EXCEL";

		// ********************** SALE FEATURE
		public const string MYSELF = "MYSELF";
		public const string MANAGER = "MANAGER";
		public const string EMPLOYEE = "EMPLOYEE";
		public const string UPDATE_RESULT = "UPDATE_RESULT";
		public const string VIEW_REPORT = "VIEW_REPORT";

		// => MỤC TIÊU
		public const string MT_DIEUCHINH = "MT_DIEUCHINH";
		public const string MT_CHOTDIEUCHINH = "MT_CHOTDIEUCHINH";
		public const string MT_CHOT = "MT_CHOT";
		public const string MT_DEXUATCHINHSUA = "MT_DEXUATCHINHSUA";
		public const string MT_XEMDEXUAT = "MT_XEMDEXUAT";
		public const string MT_CAPNHATDEXUAT = "MT_CAPNHATDEXUAT";

        // => QUYỀN LỢI
        public const string QL_CHOT = "QL_CHOT";
		public const string QL_CHOTDIEUCHINH = "QL_CHOTDIEUCHINH";
		public const string QL_DEXUATCHINHSUA = "QL_DEXUATCHINHSUA";
		public const string QL_XEMDEXUAT = "QL_XEMDEXUAT";
		public const string QL_CAPNHATDEXUAT = "QL_CAPNHATDEXUAT";
		public const string QL_THUAHUONG = "QL_THUAHUONG";
        public const string QL_HISTORY = "QL_HISTORY";

        // => MỐI QUAN HỆ
        public const string MQH_CHOT = "MQH_CHOT";
		public const string MQH_CAPNHATBANGGAINS = "MQH_CAPNHATBANGGAINS";
		public const string MQH_DANHGIA = "MQH_DANHGIA";

		// => CƠ HỘI
		public const string CH_GANCOHOI = "CH_GANCOHOI";
		public const string CH_XEMCAPNHAT = "CH_XEMCAPNHAT";
		public const string CH_THEMCAPNHAT = "CH_THEMCAPNHAT";
		public const string CH_DONGCOHOI = "CH_DONGCOHOI";

		// => SALEKIT
		public const string SALEKIT_XEMPHANQUYEN = "SALEKIT_XEMPHANQUYEN";
		public const string SALEKIT_CAPNHATPHANQUYEN = "SALEKIT_CAPNHATPHANQUYEN";

        //=> Tự học
        public const string EL_NV_XEMNOIDUNG = "EL_NV_XEMNOIDUNG";
        public const string EL_NV_LAMBAIKIEMTRA = "EL_NV_LAMBAIKIEMTRA";

        //=> Quản lý học
        public const string EL_GD_TAOCHUONGTRINH = "EL_GD_TAOCHUONGTRINH";
        public const string EL_GD_CAPNHATCHUONGTRINH = "EL_GD_CAPNHATCHUONGTRINH";
        public const string EL_GD_XOACHUONGTRINH = "EL_GD_XOACHUONGTRINH";
        public const string EL_GD_THEMKHOA = "EL_GD_THEMKHOA";
        public const string EL_GD_XOAKHOA = "EL_GD_XOAKHOA";
        public const string EL_GD_THEMHOCVIEN = "EL_GD_THEMHOCVIEN";
        public const string EL_GD_XOAHOCVIEN = "EL_GD_XOAHOCVIEN";

        //=> Quản lý khóa học
        public const string EL_NCN_TAOKHOA = "EL_NCN_TAOKHOA";
        public const string EL_NCN_SUAKHOA = "EL_NCN_SUAKHOA";
        public const string EL_NCN_XOAKHOA = "EL_NCN_XOAKHOA";
        public const string EL_NCN_TAOBAIKIEMTRA = "EL_NCN_TAOBAIKIEMTRA";

        // ********************** Nhân sự
        // => THU NHẬP
        public const string TTTN_HIENTAI_MYSELF = "TTTN_HIENTAI_MYSELF";
		public const string TTTN_HIENTAI_ALL = "TTTN_HIENTAI_ALL";
		public const string TTTN_VITRI_MYSELF = "TTTN_VITRI_MYSELF";
		public const string TTTN_VITRI_ALL = "TTTN_VITRI_ALL";
		public const string TTTN_DETAIL = "TTTN_DETAIL";
	}

	public static class ListFeatureType
	{
		public static List<Feature> Data()
		{
			return new List<Feature>()
			{
				// ********************** COMMON FEATURE
				new Feature(){ Id = FeatureType.VIEW, Name = "Xem tất cả" ,Sort = 1},
				new Feature(){ Id = FeatureType.SEARCH, Name = "Tìm kiếm" , Sort = 2},
				new Feature(){ Id = FeatureType.CREATE, Name = "Thêm mới" , Sort = 3},
				new Feature(){ Id = FeatureType.UPDATE, Name = "Chỉnh sửa" , Sort = 4},
				new Feature(){ Id = FeatureType.DELETE, Name = "Xóa" , Sort = 5},
				new Feature(){ Id = FeatureType.DETAIL, Name = "Xem chi tiết" , Sort = 6},
				new Feature(){ Id = FeatureType.IMPORT_EXCEL, Name = "Nhập Excel" , Sort = 7},
				new Feature(){ Id = FeatureType.EXPORT_EXCEL, Name = "Xuất Excel" , Sort = 8},

				// ********************** SALE FEATURE
				new Feature(){ Id = FeatureType.MYSELF, Name = "Xem của tôi" , Sort = 10},
				new Feature(){ Id = FeatureType.EMPLOYEE, Name = "Xem nhân viên" , Sort = 11},
				new Feature(){ Id = FeatureType.MANAGER, Name = "Xem giám đốc" , Sort = 12},
				new Feature(){ Id = FeatureType.UPDATE_RESULT, Name = "Cập nhật kết quả" , Sort = 13},
				new Feature(){ Id = FeatureType.VIEW_REPORT, Name = "Xem báo cáo", Sort = 14},

				// => MỤC TIÊU
				new Feature(){ Id = FeatureType.MT_CHOT, Name = "Chốt đề xuất", Sort = 20 },
				new Feature(){ Id = FeatureType.MT_DIEUCHINH, Name = "Điều chỉnh mục tiêu", Sort = 21 },
				new Feature(){ Id = FeatureType.MT_DEXUATCHINHSUA, Name = "Đề xuất chỉnh sửa", Sort = 22 },
				new Feature(){ Id = FeatureType.MT_XEMDEXUAT, Name = "Xem đề xuất", Sort = 23 },
				new Feature(){ Id = FeatureType.MT_CAPNHATDEXUAT, Name = "Cập nhật đề xuất", Sort = 24 },
				new Feature(){ Id = FeatureType.MT_CHOTDIEUCHINH, Name = "Chốt cập nhật", Sort = 25 },

				// => QUYỀN LỢI
				new Feature(){ Id = FeatureType.QL_THUAHUONG, Name = "Thừa hưởng quyền lợi" , Sort = 20},
				new Feature(){ Id = FeatureType.QL_CHOT, Name = "Chốt quyền lợi" , Sort = 21},
				new Feature(){ Id = FeatureType.QL_DEXUATCHINHSUA, Name = "Đề xuất chỉnh sửa" , Sort = 22},
				new Feature(){ Id = FeatureType.QL_XEMDEXUAT, Name = "Xem đề xuất" , Sort = 23},
				new Feature(){ Id = FeatureType.QL_CAPNHATDEXUAT, Name = "Cập nhật đề xuất" , Sort = 24},
				new Feature(){ Id = FeatureType.QL_CHOTDIEUCHINH, Name = "Chốt cập nhật" ,Sort = 25},
                new Feature(){ Id = FeatureType.QL_HISTORY, Name = "Xem lịch sử", Sort = 26 },

				// => MỐI QUAN HỆ
				new Feature(){ Id = FeatureType.MQH_CHOT, Name = "Chốt đề xuất" ,Sort = 20},
				new Feature(){ Id = FeatureType.MQH_CAPNHATBANGGAINS, Name = "Cập nhật kết quả" , Sort = 21},
				new Feature(){ Id = FeatureType.MQH_DANHGIA, Name = "Đánh giá mối quan hệ" , Sort = 22},

				// => CƠ HỘI
				new Feature(){ Id = FeatureType.CH_GANCOHOI, Name = "Gắn cơ hội" , Sort = 21},
				new Feature(){ Id = FeatureType.CH_XEMCAPNHAT, Name = "Xem cập nhật" , Sort = 22},
				new Feature(){ Id = FeatureType.CH_THEMCAPNHAT, Name = "Thêm cập nhật" , Sort = 23},
				new Feature(){ Id = FeatureType.CH_DONGCOHOI, Name = "Đóng cơ hội" , Sort = 24},

				// => SALEKIT
				new Feature(){ Id = FeatureType.SALEKIT_XEMPHANQUYEN, Name = "Cấp quyền truy cập Salekit" , Sort = 20 },
				new Feature(){ Id = FeatureType.SALEKIT_CAPNHATPHANQUYEN, Name = "Chỉnh sửa tài liệu" , Sort = 21},

				// => Tự học
				new Feature(){ Id = FeatureType.EL_NV_XEMNOIDUNG, Name = "Xem nội dung" , Sort = 21 },
                new Feature(){ Id = FeatureType.EL_NV_LAMBAIKIEMTRA, Name = "Làm bài kiểm tra" , Sort = 22},

				// => Quản lý tự học
				new Feature(){ Id = FeatureType.EL_GD_TAOCHUONGTRINH, Name = "Tạo chương trình học" , Sort = 21 },
				new Feature(){ Id = FeatureType.EL_GD_CAPNHATCHUONGTRINH, Name = "Cập nhật chương trình học" , Sort = 22 },
				new Feature(){ Id = FeatureType.EL_GD_XOACHUONGTRINH, Name = "Xóa chương trình học" , Sort = 23 },
                new Feature(){ Id = FeatureType.EL_GD_THEMKHOA, Name = "Thêm và chỉnh sửa khoá học vào chương trình" , Sort = 24},
                new Feature(){ Id = FeatureType.EL_GD_XOAKHOA, Name = "Xóa khóa học khỏi chương trình" , Sort = 25 },
                new Feature(){ Id = FeatureType.EL_GD_THEMHOCVIEN, Name = "Thêm học viên" , Sort = 26},
                new Feature(){ Id = FeatureType.EL_GD_XOAHOCVIEN, Name = "Xóa học viên" , Sort = 27 },
              
				// => Quản lý khóa học
				new Feature(){ Id = FeatureType.EL_NCN_TAOKHOA, Name = "Tạo khóa học" , Sort = 21 },
                new Feature(){ Id = FeatureType.EL_NCN_SUAKHOA, Name = "Chỉnh sửa khóa học" , Sort = 22},
                new Feature(){ Id = FeatureType.EL_NCN_XOAKHOA, Name = "Xóa khóa học" , Sort = 23 },
                new Feature(){ Id = FeatureType.EL_NCN_TAOBAIKIEMTRA, Name = "Tạo bài kiểm tra" , Sort = 24},

				// ********************** Nhân sự
				// => THU NHẬP
				new Feature(){ Id = FeatureType.TTTN_HIENTAI_MYSELF, Name = "Thu nhập hiện tại của tôi" , Sort = 21},
				new Feature(){ Id = FeatureType.TTTN_HIENTAI_ALL, Name = "Thu nhập hiện tại của tất cả" , Sort = 22},
				new Feature(){ Id = FeatureType.TTTN_VITRI_MYSELF, Name = "Thu nhập vị trí của tôi" , Sort = 23},
				new Feature(){ Id = FeatureType.TTTN_VITRI_ALL, Name = "Thu nhập vị trí của tất cả" , Sort = 24},
				new Feature(){ Id = FeatureType.TTTN_DETAIL, Name = "Xem chi tiết thu nhập" , Sort = 25},
			};
		}
	}
}
