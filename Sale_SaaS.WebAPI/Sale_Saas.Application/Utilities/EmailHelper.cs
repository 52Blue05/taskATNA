using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Utilities
{
	public class EmailHelper
	{
		public static bool kiemTraDinhDangEmail(string email)
		{
			const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

			var regex = new Regex(pattern, RegexOptions.IgnoreCase);
			if (!string.IsNullOrEmpty(email.Trim()))
			{
				return regex.IsMatch(email);
			}
			else
			{
				return false;
			}
		}
	}
}
