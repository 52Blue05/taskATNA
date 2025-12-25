namespace Sale_Saas.Domain.Enums
{
    //public enum MenuDetailEnum
    //{
    //     // Module sale
    //     Sale_CH, Sale_MQH, Sale_MT, Sale_QL, Sale_SK, Sale_SK_PQ, Sale_EL_NV, Sale_EL_GD, Sale_EL_NCN,
    //     // Module nhân sự
    //     NS_TTNS, NS_TTTN, //NS_TTTC , NS_BCTN
    //                       // Module danh mục
    //     DM_GAINS, DM_KH, DM_MDQH, DM_MDV, DM_NCC, DM_NS, DM_HD, DM_DA

    //     // Module tài khoản
    //     //QLTK, QLTK_HS, QLTK_PQ, QLTK_QTBM, QLTK_ROLE, QLTK_TK,
    //}

    public static class MenuDetailMapper
    {
        public static readonly Dictionary<string, string> MenuDetailToString = new Dictionary<string, string>
              {
                  { "Sale_CH", "opportunity" },
                  { "Sale_MQH", "relationship" },
                  { "Sale_MT", "kpi" },
                  { "Sale_QL", "privileges" },
                  { "Sale_SK", "sale-kit" },
                  { "Sale_EL_NV", "e-learning" },
                  { "Sale_EL_GD", "syllabus-management" },
                  { "Sale_EL_NCN", "unit-management" },

                  { "NS_TTNS", "human-resources" },
                  { "NS_TTTN", "income" },

                  { "DM_GAINS", "questions" },
                  { "DM_KH", "customer" },
                  { "DM_MDQH", "relationship" },
                  { "DM_MDV", "service" },
                  { "DM_NCC", "supplier" },
                  { "DM_NS", "human-resource" },
                  { "DM_HD", "contract" },
                  { "DM_DA", "project" },
                  { "DM_TC", "criteria" },
              };

        //public const string Sale_CH = "opportunity";
        //public const string Sale_MQH = "ralationship";
        //public const string Sale_MT = "kpi";
        //public const string Sale_QL = "privileges";
        //public const string Sale_SK = "sale-kit";
        ////public const string Sale_SK_PQ = "Sale_SK_PQ";
        //public const string Sale_EL_NV = "elearning";
        //public const string Sale_EL_GD = "syllabus-management";
        //public const string Sale_EL_NCN = "unit-management";
        //public const string Sale_SK_PQ = "Sale_SK_PQ";


        /*public const string QLTK = "QLTK";
        public const string QLTK_HS = "QLTK_HS";
        public const string QLTK_PQ = "QLTK_PQ";
        public const string QLTK_QTBM = "QLTK_QTBM";
        public const string QLTK_ROLE = "QLTK_ROLE";
        public const string QLTK_TK = "QLTK_TK";*/

        //public const string NS_TTNS = "human-resources";
        //public const string NS_TTTN = "income";
        ////public const string NS_BCTN = "NS_BCTN";
        ////public const string NS_TTTC = "NS_TTTC";

        //public const string DM_GAINS = "questions";
        //public const string DM_KH = "customner";
        //public const string DM_MDQH = "relationship";
        //public const string DM_MDV = "service";
        //public const string DM_NCC = "supplier";
        //public const string DM_NS = "human-resource";
        //public const string DM_HD = "contract";
        //public const string DM_DA = "project";
    }
}
