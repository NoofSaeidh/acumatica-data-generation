﻿using CrmDataGeneration.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CrmDataGeneration.OpenApi.Reference
{
    #region Common

    // just extend generated classes to be more easy to use and debug
    [DebuggerTypeProxy(typeof(EntityDebuggerProxy))]
    public partial class Entity : IEntity
    {
        public override string ToString() => $"{GetType().Name}, {nameof(Id)} = {Id}";
    }

    #endregion

    #region Values Extended
    [DebuggerDisplay("{Value}")]
    public partial class StringValue : IValueWrapper<string>
    {
        public StringValue() { }
        public StringValue(string value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<string>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<string>.GetHashCode(Value);
        public override string ToString() => ValueComparer<string>.ToString(Value);
        public static bool operator ==(StringValue left, StringValue right) => ValueComparer<string>.Equals(left, right);
        public static bool operator !=(StringValue left, StringValue right) => !ValueComparer<string>.Equals(left, right);
        public static implicit operator string(StringValue value) => value?.Value;
        public static implicit operator StringValue(string value) => new StringValue(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class BooleanValue : IValueWrapper<Boolean?>
    {
        public BooleanValue() { }
        public BooleanValue(bool? value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<Boolean?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<Boolean?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<Boolean?>.ToString(Value);
        public static bool operator ==(BooleanValue left, BooleanValue right) => ValueComparer<Boolean?>.Equals(left, right);
        public static bool operator !=(BooleanValue left, BooleanValue right) => !ValueComparer<Boolean?>.Equals(left, right);
        public static implicit operator bool? (BooleanValue value) => value?.Value;
        public static implicit operator BooleanValue(bool? value) => new BooleanValue(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class DateTimeValue : IValueWrapper<DateTime?>
    {
        public DateTimeValue() { }
        public DateTimeValue(DateTime? value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<DateTime?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<DateTime?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<DateTime?>.ToString(Value);
        public static bool operator ==(DateTimeValue left, DateTimeValue right) => ValueComparer<DateTime?>.Equals(left, right);
        public static bool operator !=(DateTimeValue left, DateTimeValue right) => !ValueComparer<DateTime?>.Equals(left, right);
        public static implicit operator DateTime? (DateTimeValue value) => value?.Value;
        public static implicit operator DateTimeValue(DateTime? value) => new DateTimeValue(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class IntValue : IValueWrapper<int?>
    {
        public IntValue() { }
        public IntValue(int? value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<int?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<int?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<int?>.ToString(Value);
        public static bool operator ==(IntValue left, IntValue right) => ValueComparer<int?>.Equals(left, right);
        public static bool operator !=(IntValue left, IntValue right) => !ValueComparer<int?>.Equals(left, right);
        public static implicit operator int? (IntValue value) => value?.Value;
        public static implicit operator IntValue(int? value) => new IntValue(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class ShortValue : IValueWrapper<short?>
    {
        public ShortValue() { }
        public ShortValue(short? value)
        {
            Value = value;
        }
        short? IValueWrapper<short?>.Value { get => (short?)Value; set => Value = value; }
        public override bool Equals(object obj) => ValueComparer<int?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<int?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<int?>.ToString(Value);
        public static bool operator ==(ShortValue left, ShortValue right) => ValueComparer<int?>.Equals(left, right);
        public static bool operator !=(ShortValue left, ShortValue right) => !ValueComparer<int?>.Equals(left, right);
        public static implicit operator short? (ShortValue value) => (short?)value.Value;
        public static implicit operator ShortValue(short? value) => new ShortValue(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class ByteValue : IValueWrapper<byte?>
    {
        public ByteValue() { }
        public ByteValue(byte? value)
        {
            Value = value;
        }
        byte? IValueWrapper<byte?>.Value { get => (byte?)Value; set => Value = value; }
        public override bool Equals(object obj) => ValueComparer<int?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<int?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<int?>.ToString(Value);
        public static bool operator ==(ByteValue left, ByteValue right) => ValueComparer<int?>.Equals(left, right);
        public static bool operator !=(ByteValue left, ByteValue right) => !ValueComparer<int?>.Equals(left, right);
        public static implicit operator byte? (ByteValue value) => (byte?)value.Value;
        public static implicit operator ByteValue(byte? value) => new ByteValue(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class LongValue : IValueWrapper<long?>
    {
        public LongValue() { }
        public LongValue(long? value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<long?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<long?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<long?>.ToString(Value);
        public static bool operator ==(LongValue left, LongValue right) => ValueComparer<long?>.Equals(left, right);
        public static bool operator !=(LongValue left, LongValue right) => !ValueComparer<long?>.Equals(left, right);
        public static implicit operator long? (LongValue value) => value?.Value;
        public static implicit operator LongValue(long? value) => new LongValue(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class DoubleValue : IValueWrapper<double?>
    {
        public DoubleValue() { }
        public DoubleValue(double? value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<Double?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<Double?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<Double?>.ToString(Value);
        public static bool operator ==(DoubleValue left, DoubleValue right) => ValueComparer<Double?>.Equals(left, right);
        public static bool operator !=(DoubleValue left, DoubleValue right) => !ValueComparer<Double?>.Equals(left, right);
        public static implicit operator double? (DoubleValue value) => value?.Value;
        public static implicit operator DoubleValue(double? value) => new DoubleValue(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class DecimalValue : IValueWrapper<decimal?>, IValueWrapper<double?>
    {
        public DecimalValue() { }
        public DecimalValue(decimal? value) : this((double?)value)
        {
        }
        public DecimalValue(double? value)
        {
            Value = value;
        }
        decimal? IValueWrapper<decimal?>.Value { get => (decimal?)Value; set => Value = (double?)value; }
        public override bool Equals(object obj) => ValueComparer<double?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<double?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<double?>.ToString(Value);
        public static bool operator ==(DecimalValue left, DecimalValue right) => ValueComparer<double?>.Equals(left, right);
        public static bool operator !=(DecimalValue left, DecimalValue right) => !ValueComparer<double?>.Equals(left, right);
        public static implicit operator decimal? (DecimalValue value) => (decimal)value.Value;
        public static implicit operator DecimalValue(decimal? value) => new DecimalValue(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class GuidValue : IValueWrapper<Guid?>
    {
        public GuidValue() { }
        public GuidValue(Guid? value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<Guid?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<Guid?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<Guid?>.ToString(Value);
        public static bool operator ==(GuidValue left, GuidValue right) => ValueComparer<Guid?>.Equals(left, right);
        public static bool operator !=(GuidValue left, GuidValue right) => !ValueComparer<Guid?>.Equals(left, right);
        public static implicit operator Guid? (GuidValue value) => value?.Value;
        public static implicit operator GuidValue(Guid? value) => new GuidValue(value);
    }

    #endregion

    #region CustomFields Extended
    [DebuggerDisplay("{Value}")]
    public partial class CustomStringField
    {
        public CustomStringField() { }
        public CustomStringField(string value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomStringField v)
                return ValueComparer<string>.Equals(Value, v.Value);

            return ValueComparer<string>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<string>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<string>.ToString(Value);
        }
        public static implicit operator string(CustomStringField value) => value?.Value;
        public static implicit operator CustomStringField(string value) => new CustomStringField(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class CustomBooleanField
    {
        public CustomBooleanField() { }
        public CustomBooleanField(bool? value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomBooleanField v)
                return ValueComparer<bool?>.Equals(Value, v.Value);

            return ValueComparer<bool?>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<bool?>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<bool?>.ToString(Value);
        }
        public static implicit operator bool? (CustomBooleanField value) => value?.Value;
        public static implicit operator CustomBooleanField(bool? value) => new CustomBooleanField(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class CustomDateTimeField
    {
        public CustomDateTimeField() { }
        public CustomDateTimeField(DateTime? value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomDateTimeField v)
                return ValueComparer<DateTime?>.Equals(Value, v.Value);

            return ValueComparer<DateTime?>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<DateTime?>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<DateTime?>.ToString(Value);
        }
        public static implicit operator DateTime? (CustomDateTimeField value) => value?.Value;
        public static implicit operator CustomDateTimeField(DateTime? value) => new CustomDateTimeField(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class CustomIntField
    {
        public CustomIntField() { }
        public CustomIntField(int? value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomIntField v)
                return ValueComparer<int?>.Equals(Value, v.Value);

            return ValueComparer<int?>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<int?>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<int?>.ToString(Value);
        }
        public static implicit operator int? (CustomIntField value) => value?.Value;
        public static implicit operator CustomIntField(int? value) => new CustomIntField(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class CustomShortField
    {
        public CustomShortField() { }
        public CustomShortField(short? value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomShortField v)
                return ValueComparer<int?>.Equals(Value, v.Value);

            return ValueComparer<int?>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<int?>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<int?>.ToString(Value);
        }
        public static implicit operator short? (CustomShortField value) => (short?)value.Value;
        public static implicit operator CustomShortField(short? value) => new CustomShortField(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class CustomByteField
    {
        public CustomByteField() { }
        public CustomByteField(byte? value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomByteField v)
                return ValueComparer<int?>.Equals(Value, v.Value);

            return ValueComparer<int?>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<int?>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<int?>.ToString(Value);
        }
        public static implicit operator byte? (CustomByteField value) => (byte?)value.Value;
        public static implicit operator CustomByteField(byte? value) => new CustomByteField(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class CustomLongField
    {
        public CustomLongField() { }
        public CustomLongField(long? value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomLongField v)
                return ValueComparer<long?>.Equals(Value, v.Value);

            return ValueComparer<long?>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<long?>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<long?>.ToString(Value);
        }
        public static implicit operator long? (CustomLongField value) => value?.Value;
        public static implicit operator CustomLongField(long? value) => new CustomLongField(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class CustomDoubleField
    {
        public CustomDoubleField() { }
        public CustomDoubleField(double? value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomDoubleField v)
                return ValueComparer<double?>.Equals(Value, v.Value);

            return ValueComparer<double?>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<double?>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<double?>.ToString(Value);
        }
        public static implicit operator double? (CustomDoubleField value) => value?.Value;
        public static implicit operator CustomDoubleField(double? value) => new CustomDoubleField(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class CustomDecimalField
    {
        public CustomDecimalField() { }
        public CustomDecimalField(decimal? value) : this((double?)value)
        {
        }
        public CustomDecimalField(double? value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomDecimalField v)
                return ValueComparer<double?>.Equals(Value, v.Value);

            return ValueComparer<double?>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<double?>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<double?>.ToString(Value);
        }
        public static implicit operator decimal? (CustomDecimalField value) => (decimal)value.Value;
        public static implicit operator CustomDecimalField(decimal? value) => new CustomDecimalField(value);
    }
    [DebuggerDisplay("{Value}")]
    public partial class CustomGuidField
    {
        public CustomGuidField() { }
        public CustomGuidField(Guid? value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomGuidField v)
                return ValueComparer<Guid?>.Equals(Value, v.Value);

            return ValueComparer<Guid?>.Equals(Value, obj);
        }
        public override int GetHashCode()
        {
            return ValueComparer<Guid?>.GetHashCode(Value);
        }
        public override string ToString()
        {
            return ValueComparer<Guid?>.ToString(Value);
        }
        public static implicit operator Guid? (CustomGuidField value) => value?.Value;
        public static implicit operator CustomGuidField(Guid? value) => new CustomGuidField(value);
    }

    #endregion

    #region Debugger display
    internal class EntityDebuggerProxy
    {
        private readonly Entity _entity;
        public EntityDebuggerProxy(Entity entity)
        {
            _entity = entity;
        }
        private KeyValuePairs[] _items;
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePairs[] Items
        {
            get
            {
                if (_items != null) return _items;
                if (_entity == null)
                    return _items = new KeyValuePairs[0];

                return GetFlattenProperties(_entity, _entity.GetType()).ToArray();
            }
        }

        internal IEnumerable<KeyValuePairs> GetFlattenProperties(object obj, Type type, string prefix = "")
        {
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            foreach (var property in props)
            {
                if (!property.CanRead)
                    continue;
                object value;
                try
                {
                    value = property.GetValue(obj);
                    if (value == null)
                        continue;
                }
                catch
                {
                    continue;
                }


                if (typeof(Entity).IsAssignableFrom(property.PropertyType))
                {
                    foreach (var innerValue in GetFlattenProperties(value, property.PropertyType, prefix + property.Name + "."))
                    {
                        yield return innerValue;
                    }
                    continue;
                }

                yield return new KeyValuePairs(prefix + property.Name, value, o =>
                {
                    if (!property.CanWrite)
                        return false;
                    try
                    {
                        property.SetValue(obj, o);
                    }
                    catch
                    {
                        try
                        {
                            var valueProp = property.PropertyType.GetProperty("Value");
                            valueProp.SetValue(value, o);
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    return true;
                });
            }
        }

        [DebuggerDisplay("{Value}", Name = "{Key}", Type = "")]
        internal class KeyValuePairs
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly Func<object, bool> _setValue;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private object _value;
            public KeyValuePairs(object key, object value, Func<object, bool> setValue = null)
            {
                Key = key;
                _value = value;
                _setValue = setValue;
            }

            public object Key { get; }


            public object Value
            {
                get => _value;
                set
                {
                    if (_setValue == null)
                        throw new InvalidOperationException("Cannot set value.");
                    var success = _setValue(value);
                    if (!success)
                        throw new InvalidOperationException("Cannot set value.");
                    _value = value;
                }
            }
        }
    }
    #endregion
}
