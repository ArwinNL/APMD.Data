using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APMD.Data
{
    public static class ModelExtensions
    {
        public static bool HasMappedChanges<T>(this T current, T other)
        {
            if (current == null || other == null)
                throw new ArgumentNullException("Cannot compare null objects.");

            Type type = typeof(T);

            // Compare properties with [Column] attribute
            var propertyDifferences = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetCustomAttribute<NotMappedAttribute>() == null && p.GetCustomAttribute<ForeignKeyAttribute>() == null)
                .Any(p => !Equals(p.GetValue(current), p.GetValue(other)));

            // Fields without [NotMapped]
            var fieldDifferences = type
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<NotMappedAttribute>() == null && f.GetCustomAttribute<ForeignKeyAttribute>() == null)
                .Any(f => !Equals(f.GetValue(current), f.GetValue(other)));


            return propertyDifferences || fieldDifferences;
        }
    }
}
