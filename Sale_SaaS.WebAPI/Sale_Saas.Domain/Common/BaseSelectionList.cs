using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Domain.Common
{
	public class BaseSelectionList
	{
		public BaseSelectionList() 
		{
			Id = 0;
			MacDinh = false;
		}
		public int Id { get; set; }
		public string? Ma { set; get; }
		public string? Ten { get; set; }
		public bool MacDinh { set; get; }
	}
}
