namespace Sale_Saas.Infrastructure.Data
{
    public interface IMustHaveTenant
    {
        public string TenantId { get; set; }
    }
}
