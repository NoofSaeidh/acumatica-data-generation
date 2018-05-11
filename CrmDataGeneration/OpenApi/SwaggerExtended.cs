using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CrmDataGeneration.OpenApi.Reference
{
    // just extend generated classes to be more easy to use and debug

    public partial class Entity
    {
        public override string ToString() => $"{GetType().Name}, {nameof(Id)} = {Id}";
    }

    [DebuggerDisplay("{TValue}")]
    public abstract class Value<T> : IEquatable<T>
    {
        // to return defined value in classes - just workaround
        // to not implement equals for each derived class
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected abstract T TValue { get; }

        public bool Equals(T other)
        {
            return EqualityComparer<T>.Default.Equals(TValue, other);
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case T t:
                    return Equals(t);
                case Value<T> vt:
                    if (vt != null)
                        return Equals(vt.TValue);
                    return EqualityComparer<object>.Default.Equals(TValue, null);
                default:
                    return EqualityComparer<object>.Default.Equals(TValue, obj);
            }
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(TValue);
        }

        public override string ToString()
        {
            if (typeof(T).IsValueType)
                return TValue.ToString();
            return TValue?.ToString() ?? "";
        }
    }

    [DebuggerDisplay("{TValue}")]
    public partial class CustomField
    {
        // to return defined value in classes - just workaround
        // to not implement equals for each derived class
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected virtual object TValue { get; }

        public override bool Equals(object obj)
        {
            if(obj is CustomField cf)
            {
                return EqualityComparer<object>.Default.Equals(TValue, cf.TValue);
            }
            return EqualityComparer<object>.Default.Equals(TValue, obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<object>.Default.GetHashCode(TValue);
        }

        public override string ToString()
        {
            return TValue?.ToString() ?? "";
        }
    }

    #region Values Extended
    public partial class StringValue : Value<string>
    {
        public StringValue() { }
        public StringValue(string value)
        {
            Value = value;
        }
        protected override string TValue => Value;
        public static implicit operator string(StringValue value) => value?.Value;
        public static implicit operator StringValue(string value) => new StringValue(value);
    }
    public partial class BooleanValue : Value<bool?>
    {
        public BooleanValue() { }
        public BooleanValue(bool? value)
        {
            Value = value;
        }
        protected override bool? TValue => Value;
        public static implicit operator bool? (BooleanValue value) => value?.Value;
        public static implicit operator BooleanValue(bool? value) => new BooleanValue(value);
    }
    public partial class DateTimeValue : Value<DateTime?>
    {
        public DateTimeValue() { }
        public DateTimeValue(DateTime? value)
        {
            Value = value;
        }
        protected override DateTime? TValue => Value;
        public static implicit operator DateTime? (DateTimeValue value) => value?.Value;
        public static implicit operator DateTimeValue(DateTime? value) => new DateTimeValue(value);
    }
    public partial class IntValue : Value<int?>
    {
        public IntValue() { }
        public IntValue(int? value)
        {
            Value = value;
        }
        protected override int? TValue => Value;
        public static implicit operator int? (IntValue value) => value?.Value;
        public static implicit operator IntValue(int? value) => new IntValue(value);
    }
    public partial class ShortValue : IntValue
    {
        public ShortValue() { }
        public ShortValue(short? value)
        {
            Value = value;
        }
        public ShortValue(int? value)
        {
            Value = value;
        }
        public static implicit operator short? (ShortValue value) => (short?)value.Value;
        public static implicit operator ShortValue(short? value) => new ShortValue(value);
    }
    public partial class ByteValue : IntValue
    {
        public ByteValue() { }
        public ByteValue(byte? value)
        {
            Value = value;
        }
        public ByteValue(int? value)
        {
            Value = value;
        }
        public static implicit operator byte? (ByteValue value) => (byte?)value.Value;
        public static implicit operator ByteValue(byte? value) => new ByteValue(value);
    }
    public partial class LongValue : Value<long?>
    {
        public LongValue() { }
        public LongValue(long? value)
        {
            Value = value;
        }
        protected override long? TValue => Value;
        public static implicit operator long? (LongValue value) => value?.Value;
        public static implicit operator LongValue(long? value) => new LongValue(value);
    }
    public partial class DoubleValue : Value<double?>
    {
        public DoubleValue() { }
        public DoubleValue(double? value)
        {
            Value = value;
        }
        protected override double? TValue => Value;
        public static implicit operator double? (DoubleValue value) => value?.Value;
        public static implicit operator DoubleValue(double? value) => new DoubleValue(value);
    }
    public partial class DecimalValue : DoubleValue
    {
        public DecimalValue() { }
        public DecimalValue(decimal? value)
        {
            Value = (double)value;
        }
        public DecimalValue(double? value)
        {
            Value = value;
        }
        public static implicit operator decimal? (DecimalValue value) => (decimal)value.Value;
        public static implicit operator DecimalValue(decimal? value) => new DecimalValue(value);
    }
    public partial class GuidValue : Value<Guid?>
    {
        public GuidValue() { }
        public GuidValue(Guid? value)
        {
            Value = value;
        }
        protected override Guid? TValue => Value;
        public static implicit operator Guid? (GuidValue value) => value?.Value;
        public static implicit operator GuidValue(Guid? value) => new GuidValue(value);
    }

    #endregion

    #region CustomFields Extended
    public partial class CustomStringField
    {
        public CustomStringField() { }
        public CustomStringField(string value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator string(CustomStringField value) => value?.Value;
        public static implicit operator CustomStringField(string value) => new CustomStringField(value);
    }
    public partial class CustomBooleanField 
    {
        public CustomBooleanField() { }
        public CustomBooleanField(bool? value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator bool? (CustomBooleanField value) => value?.Value;
        public static implicit operator CustomBooleanField(bool? value) => new CustomBooleanField(value);
    }
    public partial class CustomDateTimeField
    {
        public CustomDateTimeField() { }
        public CustomDateTimeField(DateTime? value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator DateTime? (CustomDateTimeField value) => value?.Value;
        public static implicit operator CustomDateTimeField(DateTime? value) => new CustomDateTimeField(value);
    }
    public partial class CustomIntField
    {
        public CustomIntField() { }
        public CustomIntField(int? value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator int? (CustomIntField value) => value?.Value;
        public static implicit operator CustomIntField(int? value) => new CustomIntField(value);
    }
    public partial class CustomShortField
    {
        public CustomShortField() { }
        public CustomShortField(short? value)
        {
            Value = value;
        }
        public CustomShortField(int? value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator short? (CustomShortField value) => (short?)value.Value;
        public static implicit operator CustomShortField(short? value) => new CustomShortField(value);
    }
    public partial class CustomByteField
    {
        public CustomByteField() { }
        public CustomByteField(byte? value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator byte? (CustomByteField value) => (byte?)value.Value;
        public static implicit operator CustomByteField(byte? value) => new CustomByteField(value);
    }
    public partial class CustomLongField
    {
        public CustomLongField() { }
        public CustomLongField(long? value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator long? (CustomLongField value) => value?.Value;
        public static implicit operator CustomLongField(long? value) => new CustomLongField(value);
    }
    public partial class CustomDoubleField
    {
        public CustomDoubleField() { }
        public CustomDoubleField(double? value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator double? (CustomDoubleField value) => value?.Value;
        public static implicit operator CustomDoubleField(double? value) => new CustomDoubleField(value);
    }
    public partial class CustomDecimalField
    {
        public CustomDecimalField() { }
        public CustomDecimalField(decimal? value)
        {
            Value = (double)value;
        }
        public CustomDecimalField(double? value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator decimal? (CustomDecimalField value) => (decimal)value.Value;
        public static implicit operator CustomDecimalField(decimal? value) => new CustomDecimalField(value);
    }
    public partial class CustomGuidField
    {
        public CustomGuidField() { }
        public CustomGuidField(Guid? value)
        {
            Value = value;
        }
        protected override object TValue => Value;
        public static implicit operator Guid? (CustomGuidField value) => value?.Value;
        public static implicit operator CustomGuidField(Guid? value) => new CustomGuidField(value);
    }

    #endregion
}
