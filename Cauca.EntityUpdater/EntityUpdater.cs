using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cauca.EntityUpdater
{
    public class EntityUpdater<TBaseType> where TBaseType : BaseModel
    {
        private readonly List<BaseModel> entitiesToAdd = new List<BaseModel>();
        private readonly List<BaseModel> entitiesToRemove = new List<BaseModel>();

        public (List<BaseModel> entityToRemove, List<BaseModel> entityToAdd) UpdateFromList(IList updatedEntities, IList currentEntities)
        {
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
            var currentEntity = currentEntities.OfType<TBaseType>().SingleOrDefault(e => e.Id == entity.Id);
            if (currentEntity != null)
                UpdateEntity(entity, currentEntity);
            else
                entitiesToAdd.Add(entity);
        }

        protected virtual void UpdateCopyableCollections(BaseModel source, BaseModel destination)
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
            foreach (var updatedEntity in source.OfType<BaseModel>())
            {
                AddOrUpdateEntity(destination, updatedEntity);
            }

            DeactivateItemsThatAreNotInSourceList(destination, source);
        }

        protected virtual void DeactivateItemsThatAreNotInSourceList(IList current, IList updated)
        {
	        var list = GetItemsThatAreOnlyInCurrentList(current, updated);
	        foreach (var entity in list)
	        {
		        entitiesToRemove.Add(entity);
	        }

           /* var entityToValidate = current.Count > 0 ? current[0] : updated[0];

            if (entityToValidate != null && entityToValidate.GetType().GetProperties().Any(p => p.Name == "IsActive"))
            {
                var list = GetActiveItemsThatAreOnlyInCurrentList(current, updated);
                foreach (var entity in list)
                {
	                //entity.IsActive = false;
                }
            }
            else
            {
                var list = GetItemsThatAreOnlyInCurrentList(current, updated);
                foreach (var entity in list)
                {
                    entitiesToRemove.Add(entity);
                }
            }*/
        }

        private List<BaseModel> GetActiveItemsThatAreOnlyInCurrentList(IList current, IList updated)
        {
            return current.OfType<BaseModel>().Where(e => EntityIsNotInList(updated, e)/* && e.IsActive*/).ToList();
        }

        private List<BaseModel> GetItemsThatAreOnlyInCurrentList(IList current, IList updated)
        {
            return current.OfType<BaseModel>().Where(e => EntityIsNotInList(updated, e)).ToList();
        }

        protected bool EntityIsNotInList(IList updated, BaseModel e)
        {
            return updated.OfType<BaseModel>().All(n => n.Id != e.Id);
        }

        protected virtual void AddOrUpdateEntity(IList currentEntities, BaseModel source)
        {
            var destination = currentEntities.OfType<BaseModel>().SingleOrDefault(e => e.Id == source.Id);
            if (destination != null)
                UpdateEntity(source, destination);
            else
            {
                var entity = CreateCopy(source);

                entitiesToAdd.Add(entity);
                currentEntities.Add(entity);
            }
        }

        protected virtual BaseModel CreateCopy(BaseModel original)
        {
            var copy = (BaseModel)Activator.CreateInstance(original.GetType());
            PropertiesUpdater.UpdateValues(original, copy);
            UpdateCopyableCollections(original, copy);
            return copy;
        }

        protected virtual void UpdateEntity(BaseModel source, BaseModel destination)
        {
	        PropertiesUpdater.UpdateValues(source, destination);
            UpdateCopyableCollections(source, destination);
        }

        protected virtual List<PropertyInfo> GetCopyableCollection(BaseModel entity)
        {
            var propertiesCollection = entity.GetType().GetProperties()
                .Where(property => property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)) && property.PropertyType.IsGenericType && property.GetValue(entity, null) != null).ToList();
            return propertiesCollection;
        }

        public static IList GetCollectionByPropertyName(object src, string propertyName)
        {
            return src.GetType().GetProperty(propertyName)?.GetValue(src, null) as IList;
        }
    }
}
