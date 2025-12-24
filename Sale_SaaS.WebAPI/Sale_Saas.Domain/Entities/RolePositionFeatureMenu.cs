using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities
{
	public class RolePositionFeatureMenu
	{
		[Key]
		public Guid Id { get; set; }
		public string? RolePositionId { get; set; }
		public RolePosition? RolePosition { get; set; }
		public Guid? FeatureMenuId { get; set; }
		public FeatureMenu? FeatureMenu { get; set; }
	}
}
