using Azure;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Sale_Saas.Domain.Enums;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sale_Saas.API
{
    public class MyHeaderFilter : IOperationFilter
    {
        private string vi_VN { get; } = LocaleEnum.vi_VN.ToString();

		public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "tenant",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    //add default value
                }
            });

			operation.Parameters.Add(new OpenApiParameter
			{
				Name = "locale",
				In = ParameterLocation.Query,
				Required = false,
				Schema = new OpenApiSchema
				{
					Type = "string",
					Default = new OpenApiString(vi_VN)
				}
			});
		}
    }
}
