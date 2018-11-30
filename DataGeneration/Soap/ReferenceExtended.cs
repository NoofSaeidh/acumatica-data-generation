using DataGeneration.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Soap
{
    #region Extensions

    public static class SoapExtensions
    {
        public static StringSearch ToSearch(this StringValue value) => new StringSearch { Value = value?.Value };
        public static IntSearch ToSearch(this IntValue value) => new IntSearch { Value = value?.Value };
        public static GuidSearch ToSearch(this GuidValue value) => new GuidSearch { Value = value?.Value };
    }

    #endregion

    #region Common

    // just extend generated classes to be more easy to use and debug
    [DebuggerTypeProxy(typeof(EntityDebuggerProxy))]
    public partial class Entity
    {
        public override string ToString() => $"{GetType().Name}";
    }

    public partial class Action
    {
        public override string ToString() => $"{GetType().Name}";

    }

    #endregion

    #region Client Extended
    public partial class DefaultSoapClient
    {
        public T Get<T>(T value) where T : Entity => (T)Get((Entity)value);
        public T[] GetList<T>(T value) where T : Entity => GetList((Entity)value).Cast<T>().ToArray();
        public T Put<T>(T value) where T : Entity => (T)Put((Entity)value);
        public async Task<T> GetAsync<T>(T value) where T : Entity => (T)await GetAsync((Entity)value);
        public async Task<T> PutAsync<T>(T value) where T : Entity => (T)await PutAsync((Entity)value);
        public async Task<T[]> GetListAsync<T>(T value) where T : Entity => (await GetListAsync((Entity)value)).Cast<T>().ToArray();
    }
    #endregion

    #region Values Extended
    [DebuggerDisplay("{DebbugerView()}")]
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
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(StringValue) && Value != null)
                return Value;
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }
    [DebuggerDisplay("{DebbugerView()}")]
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
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(BooleanValue) && Value != null)
                return Value.ToString();
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }
    [DebuggerDisplay("{DebbugerView()}")]
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
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(DateTimeValue) && Value != null)
                return Value.ToString();
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }
    [DebuggerDisplay("{DebbugerView()}")]
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
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(IntValue) && Value != null)
                return Value.ToString();
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }
    [DebuggerDisplay("{DebbugerView()}")]
    public partial class ShortValue : IValueWrapper<short?>
    {
        public ShortValue() { }
        public ShortValue(short? value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<short?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<short?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<short?>.ToString(Value);
        public static bool operator ==(ShortValue left, ShortValue right) => ValueComparer<short?>.Equals(left, right);
        public static bool operator !=(ShortValue left, ShortValue right) => !ValueComparer<short?>.Equals(left, right);
        public static implicit operator short? (ShortValue value) => (short?)value.Value;
        public static implicit operator ShortValue(short? value) => new ShortValue(value);
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(ShortValue) && Value != null)
                return Value.ToString();
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }
    [DebuggerDisplay("{DebbugerView()}")]
    public partial class ByteValue : IValueWrapper<byte?>
    {
        public ByteValue() { }
        public ByteValue(byte? value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<Byte?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<Byte?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<Byte?>.ToString(Value);
        public static bool operator ==(ByteValue left, ByteValue right) => ValueComparer<Byte?>.Equals(left, right);
        public static bool operator !=(ByteValue left, ByteValue right) => !ValueComparer<Byte?>.Equals(left, right);
        public static implicit operator byte? (ByteValue value) => (byte?)value.Value;
        public static implicit operator ByteValue(byte? value) => new ByteValue(value);
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(ByteValue) && Value != null)
                return Value.ToString();
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }
    [DebuggerDisplay("{DebbugerView()}")]
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
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(LongValue) && Value != null)
                return Value.ToString();
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }
    [DebuggerDisplay("{DebbugerView()}")]
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
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(DoubleValue) && Value != null)
                return Value.ToString();
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }
    [DebuggerDisplay("{DebbugerView()}")]
    public partial class DecimalValue : IValueWrapper<decimal?>
    {
        public DecimalValue() { }
        public DecimalValue(decimal? value)
        {
            Value = value;
        }
        public override bool Equals(object obj) => ValueComparer<Decimal?>.Equals(Value, obj);
        public override int GetHashCode() => ValueComparer<Decimal?>.GetHashCode(Value);
        public override string ToString() => ValueComparer<Decimal?>.ToString(Value);
        public static bool operator ==(DecimalValue left, DecimalValue right) => ValueComparer<Decimal?>.Equals(left, right);
        public static bool operator !=(DecimalValue left, DecimalValue right) => !ValueComparer<Decimal?>.Equals(left, right);
        public static implicit operator decimal? (DecimalValue value) => (decimal)value.Value;
        public static implicit operator DecimalValue(decimal? value) => new DecimalValue(value);
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(DecimalValue) && Value != null)
                return Value.ToString();
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }
    [DebuggerDisplay("{DebbugerView()}")]
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
        protected virtual string DebbugerView()
        {
            var type = GetType();
            if (type == typeof(GuidValue) && Value != null)
                return Value.ToString();
            var val = Value == null ? "null" : $"\"{Value}\"";
            return $"{type.Name}, Value = {val}";
        }
    }

    #endregion

    #region Additional values Extended

    public partial class StringSearch
    {
        public StringSearch() { }
        public StringSearch(string value, StringCondition condition = StringCondition.Equal, string value2 = null) : base(value)
        {
            Condition = condition;
            Value2 = value2;
        }
        public StringSearch(StringValue value) : this(value?.Value)
        {

        }
        public static explicit operator StringSearch(string value) => new StringSearch(value);
    }

    public partial class GuidSearch
    {
        public GuidSearch() { }
        public GuidSearch(Guid? value, GuidCondition condition = GuidCondition.Equal, Guid value2 = default) : base(value)
        {
            Condition = condition;
            // for some reason second value generated not as nullable
            // perhaps some bug?
            Value2 = value2;
        }
        public GuidSearch(GuidValue value) : this(value?.Value)
        {

        }
        public static explicit operator GuidSearch(Guid? value) => new GuidSearch(value);
    }

    public partial class IntSearch
    {
        public IntSearch() { }
        public IntSearch(int? value, IntCondition condition = IntCondition.Equal, int? value2 = null) : base(value)
        {
            Condition = condition;
            Value2 = value2;
        }
        public IntSearch(IntValue value) : this(value?.Value)
        {

        }
        public static explicit operator IntSearch(int? value) => new IntSearch(value);
    }


    public partial class DateTimeSearch
    {
        public DateTimeSearch() { }
        public DateTimeSearch(DateTime? value, DateTimeCondition condition = DateTimeCondition.Equal, DateTime? value2 = null) : base(value)
        {
            Condition = condition;
            Value2 = value2;
        }
        public DateTimeSearch(DateTimeValue value) : this(value?.Value)
        {

        }
        public static explicit operator DateTimeSearch(DateTime? value) => new DateTimeSearch(value);
    }

    // there some other fields. add it if required

    #endregion

    #region Debugger display
    internal class EntityDebuggerProxy
    {
        private readonly object _entity;
        public EntityDebuggerProxy(object entity)
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

                yield return new KeyValuePairs(prefix + property.Name, value);
            }
        }

        public override string ToString() => string.Join("; ", Items.Select(i => i.ToString()));

        [DebuggerDisplay("{Value}", Name = "{Key}", Type = "")]
        internal class KeyValuePairs
        {
            public KeyValuePairs(string key, object value)
            {
                Key = key;
                Value = value;
            }

            public string Key { get; }
            public object Value { get; }

            public override string ToString() => $"{Key} = {Value}";
        }
    }
    #endregion
}
