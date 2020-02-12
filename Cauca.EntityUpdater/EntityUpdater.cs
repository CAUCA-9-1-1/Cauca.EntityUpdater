using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cauca.EntityUpdater
{
    public interface IBaseEntity<T> where T : IComparable
	{
        T Id { get; set; }
        bool IsActive { get; set; }

        
	}

    /*public class ss
    {
        public string Id { get; set; }
        }

        public void d()
	    {
            new EntityUpdater<ss>().UpdateFromList(new List<ss>(), new List<ss>(), (item1, item2) => item1.id
	    } )
	    }
    }*/

    public class EntityUpdater<TBaseType> where TBaseType : class, new()
    {
        private readonly List<TBaseType> entitiesToAdd = new List<TBaseType>();
        private readonly List<TBaseType> entitiesToRemove = new List<TBaseType>();
        private Func<TBaseType, TBaseType, bool> keyIsSameFunc;

        public (List<TBaseType> entityToRemove, List<TBaseType> entityToAdd) UpdateFromList(
	        IList updatedEntities, IList currentEntities, Func<TBaseType, TBaseType, bool> keyIsSameFunc)
        {
	        this.keyIsSameFunc = keyIsSameFunc;
            AddOrUpdateEntities(updatedEntities, currentEntities);
            DeactivateItemsThatAreNotInSourceList(currentEntities, updatedEntities);

            return (entitiesToRemove, entitiesToAdd);
        }

        private void AddOrUpdateEntities(IList updatedEntities, IList currentEntities)
        {
            foreach (var entity in updatedEntities.OfType<TBaseType>())
                AddOrUpdateEntryEntity(currentEntities, entity);
        }

        private void AddOrUpdateEntryEntity(IList currentEntities, TBaseType entity)
        {
            var currentEntity = currentEntities.OfType<TBaseType>().SingleOrDefault(e => keyIsSameFunc(e, entity));
            if (currentEntity != null)
                UpdateEntity(entity, currentEntity);
            else
                entitiesToAdd.Add(entity);
        }

        protected virtual void UpdateCopyableCollections(TBaseType source, TBaseType destination)
        {
            foreach (var property in GetCopyableCollection(source))
            {
                var collectionSource = GetCollectionByPropertyName(source, property.Name);
                var collectionDestination = GetCollectionByPropertyName(destination, property.Name);
                if (collectionSource != null && collectionDestination != null)
                    UpdateCollection(collectionDestination, collectionSource);
            }
        }

        protected virtual void UpdateCollection(IList destination, IList source)
        {
            foreach (var updatedEntity in source.OfType<TBaseType>())
            {
                AddOrUpdateEntity(destination, updatedEntity);
            }

            DeactivateItemsThatAreNotInSourceList(destination, source);
        }

        protected virtual void DeactivateItemsThatAreNotInSourceList(IList current, IList updated)
        {
            var entityToValidate = current.Count > 0 ? current[0] : updated[0];

            if (entityToValidate != null && entityToValidate.GetType().GetProperties().Any(p => p.Name == "IsActive"))
            {
                var list = GetActiveItemsThatAreOnlyInCurrentList(current, updated);
                foreach (var entity in list)
                {
	                PropertyInfo propertyInfo = entityToValidate.GetType().GetProperty("IsActive");
                    propertyInfo.SetValue(entity, false);
                }
            }
            else
            {
                var list = GetItemsThatAreOnlyInCurrentList(current, updated);
                foreach (var entity in list)
                {
                    entitiesToRemove.Add(entity);
                }
            }
        }

        private List<TBaseType> GetActiveItemsThatAreOnlyInCurrentList(IList current, IList updated)
        {
            return current.OfType<TBaseType>().Where(e => EntityIsNotInList(updated, e) && IsPropertyActive(e) && ).ToList();
        }

        private bool IsPropertyActive(TBaseType entity)
        {
	        var isActiveValue = GetPropertyValueByPropertyName(entity, "IsActive");
	        if (isActiveValue != null)
		        return isActiveValue;
        }

        private static object GetPropertyValueByPropertyName(object obj, string propertyName)
        {
	        return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }

        private List<TBaseType> GetItemsThatAreOnlyInCurrentList(IList current, IList updated)
        {
            return current.OfType<TBaseType>().Where(e => EntityIsNotInList(updated, e)).ToList();
        }

        protected bool EntityIsNotInList(IList updated, TBaseType e)
        {
            return updated.OfType<TBaseType>().All(n => !keyIsSameFunc(n, e));
        }

        protected virtual void AddOrUpdateEntity(IList currentEntities, TBaseType source)
        {
            var destination = currentEntities.OfType<TBaseType>().SingleOrDefault(e => keyIsSameFunc(e, source));
            if (destination != null)
                UpdateEntity(source, destination);
            else
            {
                var entity = CreateCopy(source);

                entitiesToAdd.Add(entity);
                currentEntities.Add(entity);
            }
        }

        protected virtual TBaseType CreateCopy(TBaseType original)
        {
            var copy = (TBaseType)Activator.CreateInstance(original.GetType());
            UpdateValues(original, copy);
            UpdateCopyableCollections(original, copy);
            return copy;
        }

        protected virtual void UpdateEntity(TBaseType source, TBaseType destination)
        {
            UpdateValues(source, destination);
            UpdateCopyableCollections(source, destination);
        }

        protected virtual List<PropertyInfo> GetCopyableCollection(TBaseType entity)
        {
            var propertiesCollection = entity.GetType().GetProperties()
                .Where(property => property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)) && property.PropertyType.IsGenericType && property.GetValue(entity, null) != null).ToList();
            return propertiesCollection;
        }

        public static IList GetCollectionByPropertyName(object src, string propertyName)
        {
	        return GetPropertyValueByPropertyName(src, propertyName) as IList;
        }

        protected void UpdateValues(TBaseType source, TBaseType destination)
        {
            var entityType = source.GetType();
            var sourceProps = GetNonNavigationReadableProperties(entityType);
            var destinationProps = GetNonNavigationWritableProperties(entityType);

            CopyProperties(source, destination, sourceProps, destinationProps);
        }

        private static void CopyProperties(TBaseType source, TBaseType destination, PropertyInfo[] sourceProps, PropertyInfo[] destinationProps)
        {
            foreach (var sourceProp in sourceProps)
            {
                CopyProperty(source, destination, destinationProps, sourceProp);
            }
        }

        private static void CopyProperty(TBaseType source, TBaseType destination, PropertyInfo[] destinationProps, PropertyInfo sourceProp)
        {
            var p = destinationProps.FirstOrDefault(x => x.Name == sourceProp.Name);
            if (p != null && p.CanWrite)
            {
                p.SetValue(destination, sourceProp.GetValue(source, null), null);
            }
        }

        protected PropertyInfo[] GetNonNavigationWritableProperties(Type entityType)
        {
            return entityType.GetProperties()
                .Where(p => p.CanWrite && !IsNavigationProperty(entityType, p))
                .ToArray();
        }

        protected PropertyInfo[] GetNonNavigationReadableProperties(Type entityType)
        {
            return entityType.GetProperties()
                .Where(p => p.CanRead && !IsNavigationProperty(entityType, p))
                .ToArray();
        }

        protected static bool IsNavigationProperty(Type entityType, PropertyInfo p)
        {
            return (typeof(IEnumerable).IsAssignableFrom(p.PropertyType) || p.PropertyType.IsClass) && p.PropertyType != typeof(string);
        }
    }
}
