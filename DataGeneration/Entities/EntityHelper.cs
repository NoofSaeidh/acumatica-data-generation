using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities
{
    public static class EntityHelper
    {
        private static readonly string _namespace = typeof(Entity).Namespace;
        public static Entity InitializeFromType(string entityType)
        {
            var type = entityType.Contains('.')
                        ? entityType
                        : _namespace + '.' + entityType;
            object res;
            try
            {
                res = Activator.CreateInstance(null, type).Unwrap();
            }
            catch(Exception e)
            {
                throw new InvalidOperationException($"Cannot initialize object of type {type}.", e);
            }
            if (res is Entity entity)
                return entity;

            throw new InvalidOperationException($"Object of specified type {type} is not inherit Entity.");
        }

        public static void SetPropertyValue(Entity entity, string propertyName, object value)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var entityType = entity.GetType();
            var property = entityType.GetProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"Entity {entityType.Name} doesn't contain property {propertyName}.");

            try
            {
                property.SetValue(entity, value);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Cannot set value \"{propertyName}\" of entity \"{entityType.Name}\".", e);
            }
        }

        public static object GetPropertyValue(Entity entity, string propertyName)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var entityType = entity.GetType();
            var property = entityType.GetProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"Entity {entityType.Name} doesn't contain property {propertyName}.");

            try
            {
                return property.GetValue(entity);
            }
            catch(Exception e)
            {
                throw new InvalidOperationException($"Cannot get value \"{propertyName}\" of entity \"{entityType.Name}\".", e);
            }
        }
    }
}
