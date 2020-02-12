using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Cause.CustomerPortal.ServiceLayer.Base
{
    public static class PropertiesUpdater
    {
        public static void UpdateValues<T>(T source, T destination)
        {
            var entityType = source.GetType();
            var sourceProps = GetNonNavigationReadableProperties(entityType);
            var destinationProps = GetNonNavigationWritableProperties(entityType);

            CopyProperties(source, destination, sourceProps, destinationProps);
        }

        private static void CopyProperties<T>(T source, T destination, PropertyInfo[] sourceProps, PropertyInfo[] destinationProps)
        {
            foreach (var sourceProp in sourceProps)
            {
                CopyProperty<T>(source, destination, destinationProps, sourceProp);
            }
        }

        private static void CopyProperty<T>(T source, T destination, PropertyInfo[] destinationProps, PropertyInfo sourceProp)
        {
            var p = destinationProps.FirstOrDefault(x => x.Name == sourceProp.Name);
            if (p != null && p.CanWrite)
            {
                p.SetValue(destination, sourceProp.GetValue(source, null), null);
            }
        }

        public static PropertyInfo[] GetNonNavigationWritableProperties(Type entityType)
        {
            return entityType.GetProperties()
                .Where(p => p.CanWrite && !IsNavigationProperty(entityType, p))
                .ToArray();
        }

        public static PropertyInfo[] GetNonNavigationReadableProperties(Type entityType)
        {
            return entityType.GetProperties()
                .Where(p => p.CanRead && !IsNavigationProperty(entityType, p))
                .ToArray();
        }

        public static bool IsNavigationProperty(Type entityType, PropertyInfo p)
        {
            return (typeof(IEnumerable).IsAssignableFrom(p.PropertyType) || p.PropertyType.IsClass) && p.PropertyType != typeof(string);
        }
    }
}
