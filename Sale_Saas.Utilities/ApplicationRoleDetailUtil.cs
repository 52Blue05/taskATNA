using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Utilities
{
    public static class ApplicationRoleDetailUtil
    {
        public static string ConvertToString(List<ApplicationRoleDetailDto> applicationRoleDetails)
        {
            string result = string.Empty;

            if(applicationRoleDetails != null)
            {
                foreach (ApplicationRoleDetailDto item in applicationRoleDetails)
                {
                    result += item.NameController + "," + item.NameAction + "," + item.Permision.ToString() + "|";
                }
            }            
            
            return result;
        }

        public static List<ApplicationRoleDetailDto>? ConvertToApplicationRoleDetails(string applicationRoleDetails)
        {
            List<ApplicationRoleDetailDto> result = null;

            if(!string.IsNullOrEmpty(applicationRoleDetails))
            {
                result = new List<ApplicationRoleDetailDto>();
                List<string> dataRows = applicationRoleDetails.Split('|').ToList();
                foreach (string dataRow in dataRows)
                {
                    if(!string.IsNullOrEmpty(dataRow))
                    {
                        List<string> data = dataRow.Split(",").ToList();
                        result.Add(new ApplicationRoleDetailDto()
                        {
                            NameController = data[0],
                            NameAction = data[1],
                            Permision = int.Parse(data[2])
                        });
                    }                    
                }
            }    

            return result;
        }
    }
}
