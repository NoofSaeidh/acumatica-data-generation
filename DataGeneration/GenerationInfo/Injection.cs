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

        public PropertiesInjectionSettings PropertiesInjectionSettings { get; set; }

        public JToken Value { get; set; }

        public JObject Properties { get; set; }

        public List<JsonInjection> Inner { get; set; }

        public void Inject(object source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var originalValue = GetPropertyValueByPath(source, Path, out var propertyName, out var parent);
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
                    if (originalValue != null)
                        ApplyProperties(originalValue, Properties);

                    break;
                }
                case InjectionType.PatchOrAdd:
                {
                    if (originalValue != null)
                        goto case InjectionType.Patch;
                    else
                        goto case InjectionType.Add;
                }
                case InjectionType.Replace:
                {
                    newValue = Value;
                    if (Value != null)
                        SetPropertyValue(parent, propertyName, Value);
                    else
                        SetPropertyNewValue(parent, propertyName, out newValue);

                    ApplyProperties(newValue, Properties);
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

        private void ApplyProperties(object source, JObject properties)
        {
            if (properties == null || properties.Count == 0)
                return;

            JsonConvert.PopulateObject(properties.ToString(), source, GeneratorConfig.ConfigJsonSettings);
        }

        private object GetPropertyValueByPath(object source, string path, out string propertyName, out object parent)
        {
            if(path.IsNullOrEmpty())
            {
                parent = null;
                propertyName = null;
                return source;
            }

            var paths = Path.Split('.');
            object result = null;
            for (int i = 0; i < paths.Length; i++)
            {
                if (result == null && i != 0)
                    throw new InvalidOperationException($"Cannot get property of source by specified path. " +
                        $"Path = {path}. Source = {source}.");

                var tmp = GetPropertyValue(source, paths[i]);
                source = result;
                result = tmp;
            }
            propertyName = paths[paths.Length - 1];
            parent = source;
            return result;
        }

        private object GetPropertyValue(object source, string propertyName)
        {
            try
            {
                return source.GetType().GetProperty(propertyName).GetValue(source);
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

                prop.SetValue(source, value);
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
        Add, // if null - add, otherwise - do nothing
        Patch, // patch only properties
        PatchOrAdd, // add if origin value is null
        Replace,
        Remove,
    }

    public enum PropertiesInjectionSettings
    {
        IgnoreMissmatch = 0,
        ThrowOnMissmatch = 1,
    }
}