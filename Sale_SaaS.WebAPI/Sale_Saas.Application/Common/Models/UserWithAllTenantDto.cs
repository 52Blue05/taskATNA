using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Common.Models;

public class UserWithAllTenantDto
{
    public string TenantId { get; set; }
    public string TenantName { get; set; }
    public string? ConnectString { get; set; }
    public string? Logo { get; set; }
    public Guid? ApplicationUserId { get; set; }
    public int? Count { get; set; }
}
