using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Domain.Constants.API
{
    public class MailServiceConstant
    {
        public string ConfigMailServer_Host { set; get; }
        public string ConfigMailServer_Email{ set; get; }
        public string ConfigMailServer_Password { set; get; }
        public int ConfigMailServer_Port { set; get; }
        public bool ConfigMailServer_SSL { set; get; }
    }
}
