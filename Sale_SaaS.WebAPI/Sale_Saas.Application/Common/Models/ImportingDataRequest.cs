using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Common.Models
{
    public class ImportingDataRequest
    {
        public System.Data.DataTable? Dt { get; init; }
        public Guid? CreatedApplicationUserId { get; init; }
    }
}
