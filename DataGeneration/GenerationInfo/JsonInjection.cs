using DataGeneration.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DataGeneration.GenerationInfo
{
    public class JsonInjection
    {
        private static readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(GeneratorConfig.ConfigJsonSettings);

        public string Path { get; set; }

        public InjectionType Type { get; set; }

        public PropertyNotFoundAction PropertyNotFound { get; set; }

        public JToken Value { get; set; }

        public List<JsonInjection> Inner { get; set; }

        public void Inject(object source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var originalValue = GetPropertyValueByPath(source, Path, out var propertyName, out var parent, out var notFound);
            if(notFound)
            {
                if (PropertyNotFound == PropertyNotFoundAction.Throw)
                    throw new InvalidOperationException($"Cannot find property by specified path. Path = {Path}, Source = {source}.");
                return;
            }
            var newValue = originalValue;
            switch (Type)
            {
                case InjectionType.Add:
                {
                    if (originalValue != null)
                        break;
                    goto case InjectionType.Replace;
                }
                case InjectionType.Patch:
                {
                    if (Value == null)
                        throw new InvalidOperationException("Patch cannot be applied with null value.");
                    if (originalValue != null)
                        Patch(originalValue, Value);

                    break;
                }
                case InjectionType.PatchOrAdd:
                {
                    if (originalValue != null)
                        goto case InjectionType.Patch;
                    else
                        goto case InjectionType.Replace;
                }
                case InjectionType.Replace:
                {
                    newValue = Value;
                    if (Value != null)
                        SetPropertyValue(parent, propertyName, Value);
                    else
                        SetPropertyNewValue(parent, propertyName, out newValue);

                    break;
                }
                case InjectionType.Remove:
                {
                    newValue = null;
                    SetPropertyValue(parent, propertyName, null);
                    break;
                }
            }

            if(newValue != null && Inner != null)
            {
                foreach (var inner in Inner)
                {
                    inner.Inject(newValue);
                }
            }
        }

        public static void Inject(object source, IEnumerable<JsonInjection> injections)
        {
            if (injections == null)
                throw new ArgumentNullException(nameof(injections));

            foreach (var inj in injections)
            {
                inj.Inject(source);
            }
        }

        private void Patch(object source, JToken value)
        {
            if (value == null)
                return;

            JsonConvert.PopulateObject(value.ToString(), source, GeneratorConfig.ConfigJsonSettings);
        }

        private object GetPropertyValueByPath(object source, string path, out string propertyName, out object parent, out bool notFound)
        {
            parent = source;

            if(path.IsNullOrEmpty())
            {
                propertyName = null;
                notFound = false;
                return null;
            }

            var paths = Path.Split('.');
            object result = source;
            for (int i = 0; i < paths.Length; i++)
            {
                if(result == null)
                {
                    notFound = true;
                    propertyName = null;
                    return null;
                }
                var tmp = GetPropertyValue(result, paths[i], out var notFound_);
                if (notFound_)
                {
                    notFound = true;
                    propertyName = null;
                    return null;
                }
                parent = result;
                result = tmp;
            }
            propertyName = paths[paths.Length - 1];
            notFound = false;
            return result;
        }

        private object GetPropertyValue(object source, string propertyName, out bool notFound)
        {
            try
            {
                var property = source.GetType().GetProperty(propertyName);
                if(property == null)
                {
                    notFound = true;
                    return null;
                }
                notFound = false;
                return property.GetValue(source);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Cannot get property. " +
                    $"Source = {source}, Property = {propertyName}.", e);
            }
        }

        private void SetPropertyValue(object source, string propertyName, JToken value)
        {
            try
            {
                var prop = source.GetType().GetProperty(propertyName);
                // it is not good approach
                // todo: refactor this
                var v = value.ToObject(prop.PropertyType, _jsonSerializer);

                prop.SetValue(source, v);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Cannot set property. " +
                    $"Source = {source}, Property = {propertyName}, Value = {value}.", e);
            }
        }

        private void SetPropertyNewValue(object source, string propertyName, out object value)
        {
            try
            {
                var property = source.GetType().GetProperty(propertyName);
                value = Activator.CreateInstance(property.PropertyType, true);
                property.SetValue(source, value);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Cannot create and set property. " +
                    $"Source = {source}, Property = {propertyName}.", e);
            }
        }
    }

    // just for obviousness
    public class JsonInjection<T> : JsonInjection
    {
        public void Inject(T source)
        {
            base.Inject(source);
        }
    }

    public enum InjectionType
    {
        Replace = 0,
        Add = 1, // if null - add, otherwise - do nothing
        Patch = 2, // patch only properties
        PatchOrAdd = 3, // add if origin value is null
        Remove = 4,
    }

    public enum PropertyNotFoundAction
    {
        Throw = 0,
        Ignore = 1,
    }
}