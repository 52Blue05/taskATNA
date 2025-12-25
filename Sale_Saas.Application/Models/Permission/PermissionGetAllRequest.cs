namespace Sale_Saas.Application.Models.Permission
{
    public class PermissionGetAllRequest
    {
        public string? TextSearch { get; set; }
        public string? OrderCol { get; set; }
        public string? OrderDir { get; set; }
        public List<Guid>? Ids { get; set; }
    }
}
