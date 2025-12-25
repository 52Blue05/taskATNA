using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Interfaces.Services
{
    public interface ILoggerService
    {
        Task WriteErrorLogAsync(Exception exception, HttpRequest request = null, string functionName = "", object requestObject = null);
        void WriteErrorLog(Exception exception, HttpRequest request = null, string functionName = "");
        void WriteInfoLog(string logMessage, HttpRequest request = null, string functionName = "");
        void WriteWarningLog(string logMessage, HttpRequest request = null, string functionName = "");
    }
}
