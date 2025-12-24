using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Domain.Entities
{
    public class EventLog : BaseAuditableEntity
    {
        public string? Code { set; get; }
        public string? Name { set; get; }
        public string? Action { set; get; }
        public string? Notes { set; get; }
        public string? From { set; get; }
        public string? To { set; get; }
    }
}
