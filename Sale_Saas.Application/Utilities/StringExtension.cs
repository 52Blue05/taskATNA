namespace Sale_Saas.Application.Utilities
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(string?[] list)
        {
            bool result = false;
            foreach (var item in list) {
                if (string.IsNullOrEmpty(item))
                {
                    result = true;
                }
            }
            return result;
        }

        public static bool IsNullOrWhiteSpace(string?[] list)
        {
            bool result = false;
            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
