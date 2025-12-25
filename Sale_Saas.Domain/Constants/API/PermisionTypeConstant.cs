namespace Sale_Saas.Domain.Constants.API
{
    public class PermisionTypeConstant
    {
        public int ViewAll { set; get; } = 0;
        public int ViewProfile { set; get; } = 1;
        public int Update { set; get; } = 2;
        public int Delete { set; get; } = 3;
        public int DownloadExcel { set; get; } = 4;
        public int DownloadWord { set; get; } = 5;
        public int Import { get; set; } = 6;
        public int ViewEmployees { get; set; } = 7; 
        public int ViewManager { get; set; } = 8;
        public int Create { set; get; } = 9;
        public int UpdateResult { set; get; } = 10;
    }

    public static class PermissionType
    {
        public static PermisionTypeConstant permission = new PermisionTypeConstant();

        public static readonly Dictionary<int, string> Data = new Dictionary<int, string>
        {
            { permission.ViewAll , "Xem tất cả" },
            { permission.ViewProfile , "Xem cá nhân" },
            { permission.Create , "Thêm mới" },
            { permission.Update , "Cập nhật" },
            { permission.UpdateResult , "Cập nhật kết quả" },
            { permission.Delete , "Xóa" },
            { permission.DownloadExcel , "Download Excel" },
            { permission.DownloadWord , "Download Word" },
            { permission.Import , "Import file" },
            { permission.ViewEmployees , "Xem nhân viên" },
            { permission.ViewManager , "Xem giám đốc"  }
        };

        public static string GetName(int code)
        {
            return Data.TryGetValue(code, out string name) ? name : "";
        }

        public static List<Permission> GetPermissions()
        {
            var permissions = new List<Permission>();
            foreach (var kvp in Data)
            {
                permissions.Add(new Permission { Code = kvp.Key, Name = kvp.Value });
            }
            return permissions;
        }
    }

    public class Permission
    {
        public int Code { get; set; }
        public string Name { get; set; } = "";
    }
}
