using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Models.Identity
{
    public class GetListRoleWithPaginationQueryRequest : GetListWithPaginationQueryRequest
    {
        public string? RoleName { get; set; }
    }
}
