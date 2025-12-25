using Sale_Saas.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Sale_Saas.API.Middlewares
{
    public class TrackingLogMiddleware
    {
        private readonly RequestDelegate _next;

        public TrackingLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {

                var requestTime = DateTime.Now;
                var handler = new JwtSecurityTokenHandler();
                string authHeader = context.Request.Headers["Authorization"].FirstOrDefault() ?? "";

                if (!string.IsNullOrEmpty(authHeader))
                {
                    authHeader = authHeader.Replace("Bearer ", "");
                    var jsonToken = handler.ReadToken(authHeader);
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;

                    if (tokenS != null)
                    {
                        var originalBodyStream = context.Response.Body;

                        using (var responseBody = new MemoryStream())
                        {
                            context.Response.Body = responseBody;

                            Guid id = Guid.Empty;
                            string tenantId = "";
                            try
                            {
                                id = Guid.Parse(tokenS.Claims.First(claim => claim.Type == "ApplicationUserId").Value);
                                tenantId = (tokenS.Claims.First(claim => claim.Type == "Tenant").Value).ToString();
                            }
                            catch (Exception ex) { }
                            finally
                            {
                                var tempTrackingLog = new TrackingLog
                                {
                                    Id = Guid.NewGuid(),
                                    ApplicationUserId = id,
                                    CreatedApplicationUserId = id,
                                    CreatedDate = DateTime.Now,
                                    LastModifiedApplicationUserId = id,
                                    LastModifiedDate = DateTime.Now
                                };

                                await _next(context);

                                responseBody.Seek(0, SeekOrigin.Begin);
                                var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
                                responseBody.Seek(0, SeekOrigin.Begin);
                                await responseBody.CopyToAsync(originalBodyStream);

                                var responseTime = DateTime.Now;
                                float duration = (float)(responseTime - requestTime).TotalMinutes;

                                tempTrackingLog.FunctionTime = context.Request.Path;
                                tempTrackingLog.Method = context.Request.Method;
                                tempTrackingLog.ResponseTimeSec = duration * 60;
                                tempTrackingLog.ResponseTimeMin = duration;

                                try
                                {
                                    using (JsonDocument doc = JsonDocument.Parse(responseBodyText))
                                    {
                                        JsonElement root = doc.RootElement;

                                        try
                                        {
                                            var errorMessage = root.TryGetProperty("errorMessage", out JsonElement errorMessageElement);
                                            if (errorMessageElement.GetString() != "")
                                            {
                                                tempTrackingLog.Message = errorMessageElement.GetString();
                                            }
                                            else
                                            {
                                                tempTrackingLog.Message = null;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            tempTrackingLog.Message = ex.Message;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    tempTrackingLog.Message = ex.Message;
                                }

                                tempTrackingLog.Status = context.Response.StatusCode.ToString();
                                tempTrackingLog.Type = "API";


                                tempTrackingLog.TenantId = tenantId;

                                if (context.Request.Query.ContainsKey("tenant"))
                                {
                                    tempTrackingLog.TenantId = context.Request.Query["tenant"].ToString();
                                }

                                TrackingLogQueue.Enqueue(tempTrackingLog);
                            }
                        }
                    }
                    else
                    {
                        await _next(context);
                    }
                }
                else
                {
                    await _next(context);
                }

            }
            catch (Exception ex)
            {
                await _next(context);
            }
        }
    }
}
