namespace Sale_Saas.Domain.Enums
{
    public enum MenuEnum
    {
        // Module sale
        Sale, Sale_CH, Sale_MQH, Sale_MT, Sale_QL, Sale_SK, Sale_SK_PQ,
        // Module nhân sự
        NS, NS_TTNS, NS_TTTN, //NS_TTTC , NS_BCTN
		// Module danh mục
		DM, DM_GAINS, DM_KH, DM_MDQH, DM_MDV, DM_NCC, DM_NS , DM_HD , DM_DA

		// Module tài khoản
		//QLTK, QLTK_HS, QLTK_PQ, QLTK_QTBM, QLTK_ROLE, QLTK_TK,
	}

	public static class MenuType
    {
        public const string Sale = "Sale";
		public const string Sale_CH = "Sale_CH";
		public const string Sale_MQH = "Sale_MQH";
		public const string Sale_MT = "Sale_MT";
		public const string Sale_QL = "Sale_QL";
		public const string Sale_SK = "Sale_SK";
		//public const string Sale_SK_PQ = "Sale_SK_PQ";
		public const string Sale_EL_NV = "Sale_EL_NV";
		public const string Sale_EL_GD = "Sale_EL_GD";
		public const string Sale_EL_NCN = "Sale_EL_NCN";
		//public const string Sale_SK_PQ = "Sale_SK_PQ";


		/*public const string QLTK = "QLTK";
		public const string QLTK_HS = "QLTK_HS";
		public const string QLTK_PQ = "QLTK_PQ";
		public const string QLTK_QTBM = "QLTK_QTBM";
		public const string QLTK_ROLE = "QLTK_ROLE";
		public const string QLTK_TK = "QLTK_TK";*/


		public const string NS = "NS";
		public const string NS_TTNS = "NS_TTNS";
		public const string NS_TTTN = "NS_TTTN";
		//public const string NS_BCTN = "NS_BCTN";
		//public const string NS_TTTC = "NS_TTTC";

		public const string DM = "DM";
		public const string DM_GAINS = "DM_GAINS";
		public const string DM_KH = "DM_KH";
		public const string DM_MDQH = "DM_MDQH";
		public const string DM_MDV = "DM_MDV";
		public const string DM_NCC = "DM_NCC";
		public const string DM_NS = "DM_NS";
		public const string DM_HD = "DM_HD";
		public const string DM_DA = "DM_DA";
        public const string DM_TC = "DM_TC";
    }
}