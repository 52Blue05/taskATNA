using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Common.Models
{
    public class UpdateProfileRequest
    {
        public Guid ApplicationUserId {  get; set; }
        public IFormFile? Avatar { get; set; }
        public string? FullName { get; set; }
    }
}
