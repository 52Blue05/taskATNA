using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sale_Saas.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services.Identity
{
    public class LoggerService:ILoggerService
    {
        private readonly string _slackWebHooksUrl;
        private readonly ILogger<LoggerService> _logger;

        public LoggerService(IConfiguration config, ILogger<LoggerService> logger)
        {
            _slackWebHooksUrl = config["SlackWebHooks"];
            _logger = logger;
        }

        public async Task WriteErrorLogAsync(Exception exception, HttpRequest request = null, string functionName = "", object requestObject = null)
        {
            try
            {
                var endpointName = CreateHeaderMsg(request, functionName);
                var msg = exception.Message;
                if (requestObject != null)
                {
                    var contentString = JsonConvert.SerializeObject(requestObject, Formatting.Indented);
                    msg = $"{msg}, RequestInfo: {contentString}";
                }
                _logger.LogError(exception, $"Error at {endpointName}, msg: {msg}");
                await PushErrorToSlackHookAsync(endpointName, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error at WriteErrorLogAsync");
            }
        }

        public void WriteErrorLog(Exception exception, HttpRequest request = null, string functionName = "")
        {
            try
            {
                var endpointName = CreateHeaderMsg(request, functionName);
                _logger.LogError(exception, $"Error at {endpointName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error at WriteErrorLog");
            }
        }

        public void WriteInfoLog(string logMessage, HttpRequest request = null, string functionName = "")
        {
            try
            {
                var endpointName = CreateHeaderMsg(request, functionName);
                _logger.LogInformation($"Info at {endpointName}, msg: {logMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error at WriteInfoLog");
            }
        }

        public void WriteWarningLog(string logMessage, HttpRequest request = null, string functionName = "")
        {
            try
            {
                var endpointName = CreateHeaderMsg(request, functionName);
                _logger.LogWarning($"Warning at {endpointName}, msg: {logMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error at WriteWarningLog");
            }
        }

        #region "Private method"
        private async Task PushErrorToSlackHookAsync(string endpointName, string messages)
        {
            try
            {
                if (string.IsNullOrEmpty(_slackWebHooksUrl))
                {
                    _logger.LogWarning("SlackWebHooks setting is empty, please check config");
                    return;
                }

                var dataPost = new Dictionary<string, string>()
                {
                    { "text", $"Error at {endpointName}, msg: {messages}" }
                };

                var content = new StringContent(JsonConvert.SerializeObject(dataPost), Encoding.UTF8, "application/json");
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(_slackWebHooksUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();
                if (!responseString.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogWarning($"Push messages to slack not ok: {responseString}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error at PushToSlackHook");
            }
        }

        private string CreateHeaderMsg(HttpRequest request = null, string functionName = "")
        {
            var endpointName = (request != null) ? request.Path.Value : string.Empty;
            if (!string.IsNullOrEmpty(functionName)) endpointName += $" - func: {functionName}";
            return endpointName;
        }
        #endregion "End Private method"
    }
}

