namespace Sale_Saas.Application.Models.Lession
{
    public class LessionAddOrUpdateRequest
    {
        public Guid? Id { get; set; }
        public IFormFile? File { get; set; }
        public string? Name { get; set; }
        public string? Link { get; set; }
        public Guid UnitId { get; set; }
        public int? SortOrder { get; set; }
    }
}
