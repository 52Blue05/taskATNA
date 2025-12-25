using System.Reflection;

namespace Sale_Saas.Application.Utilities
{
    public static class PropertiesExtension
    {
        public static void Copy(object source, object destination)
        {
            PropertyInfo[] sourceProperties = source.GetType().GetProperties();
            PropertyInfo[] destProperties = destination.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                foreach (var destProperty in destProperties)
                {
                    if (destProperty.Name == sourceProperty.Name && destProperty.PropertyType == sourceProperty.PropertyType && destProperty.CanWrite)
                    {
                        destProperty.SetValue(destination, sourceProperty.GetValue(source));
                        break;
                    }
                }
            }
        }

        public static object GetEmpty(object obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.CanWrite);

            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;

                if (propertyType == typeof(string))
                {
                    property.SetValue(obj, string.Empty);
                }
                else if (propertyType == typeof(Guid))
                {
                    property.SetValue(obj, Guid.Empty);
                }
                else if (propertyType == typeof(bool))
                {
                    property.SetValue(obj, false);
                }
                else if (propertyType == typeof(DateTime))
                {
                    property.SetValue(obj, new DateTime());
                }
                else if (propertyType.IsValueType)
                {
                    property.SetValue(obj, Activator.CreateInstance(propertyType));
                }
                else
                {
                    property.SetValue(obj, null);
                }
            }

            return obj;
        }

    }
}
