using System.Dynamic;
using System.Reflection;

namespace PRN232.Lab2.CoffeeStore.API.Helpers
{
    public static class FieldSelector
    {
        public static object SelectFields<T>(T obj, string? selectFields)
        {
            if (string.IsNullOrWhiteSpace(selectFields) || obj == null)
            {
                return obj!;
            }

            var fields = selectFields.Split(',').Select(f => f.Trim().ToLower()).ToList();
            var expando = new ExpandoObject() as IDictionary<string, object>;
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (fields.Contains(property.Name.ToLower()))
                {
                    var value = property.GetValue(obj);
                    expando[property.Name] = value!;
                }
            }

            return expando;
        }

        public static List<object> SelectFields<T>(List<T> list, string? selectFields)
        {
            return list.Select(item => SelectFields(item, selectFields)).ToList();
        }
    }
}
