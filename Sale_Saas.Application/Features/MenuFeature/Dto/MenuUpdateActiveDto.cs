using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.MenuFeature.Dto
{
    public class MenuUpdateActiveDto
    {
        public Guid? LastModifiedApplicationUserId { set; get; }
        public List<string> Modules { set; get; }
        public string? ConnectString { set; get; }
    }
}
