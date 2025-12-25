using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Utilities
{
    public static class FunctionUtilServices
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> q, string SortField, bool Ascending)
        {
            try
            {
                //foreach (PropertyInfo propertyInfo in q.GetType().GetProperties())
                //{
                //    if (propertyInfo.Name != SortField)
                //    {
                //        return q;
                //    }
                //}
                SortField = SortField.Substring(0, 1).ToUpper() + SortField.Substring(1, SortField.Length - 1);
                var param = Expression.Parameter(typeof(T), "p");
                var prop = Expression.Property(param, SortField);
                var exp = Expression.Lambda(prop, param);
                string method = Ascending ? "OrderBy" : "OrderByDescending";
                Type[] types = new Type[] { q.ElementType, exp.Body.Type };
                var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
                return q.Provider.CreateQuery<T>(mce);
            }
            catch (Exception ex)
            {
                return q;
            }
        }

        public static IEnumerable<T> OrderObjectByDynamic<T>(this IEnumerable<T> source, string propertyName, bool Ascending)
        {
            try
            {
                foreach (PropertyInfo propertyInfo in source.GetType().GetProperties())
                {
                    if (propertyInfo.Name != propertyName)
                    {
                        return source;
                    }
                }
                propertyName = propertyName.Substring(0, 1).ToUpper() + propertyName.Substring(1, propertyName.Length - 1);
                if (Ascending)
                    return source.OrderBy(x => x.GetType().GetProperty(propertyName).GetValue(x, null));
                else
                    return source.OrderByDescending(x => x.GetType().GetProperty(propertyName).GetValue(x, null));
            }
            catch (Exception ex)
            {
                return source;
            }

        }
    }
}
