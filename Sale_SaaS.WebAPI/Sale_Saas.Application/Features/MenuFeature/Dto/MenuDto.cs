
namespace Sale_Saas.Application.Features.MenuFeature.Dto
{
    public class MenuDto: BaseEntityDto
    {
        public int SortOder { set; get; }
		public string? Code { set; get; }
		public string? Name { set; get; }
		public string? NameAction { set; get; }
        public string? NameController { set; get; }
        public string? Parameter { set; get; }
		public string? BreadcrumbNavigation { set; get; }
		public bool? IsActivite { set; get; }
		public Guid? ParentId { set; get; }
        public string? Icon { set; get; }
        public string? Link { set; get; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Menu, MenuDto>();
            }
        }
    }

	public class CategoryMenuDto : BaseEntityDto
	{
		public string Code { get; set; } = "";
		public string Icon { get; set; } = "";
		public string Path { get; set; } = "";
		public CategoryLabel Label { get; set; } = new CategoryLabel();
		public List<CategoryChildrenMenuDto> Children { get; set; } = new List<CategoryChildrenMenuDto>();
	}

	public class CategoryLabel
	{
		public string vi_VN { get; set; } = "";
		public string en_US { get; set; } = "";
	}

    public class CategoryChildrenMenuDto : BaseEntityDto
	{
		public string Code { get; set; } = "";
		public string Icon { get; set; } = "";
		public string Path { get; set; } = "";
		public CategoryLabel Label { get; set; } = new CategoryLabel();
	}


}
