using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Models.Lession
{
    public class LessionDownloadRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }
}
