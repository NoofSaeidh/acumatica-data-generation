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
        public override string ToString() => $"{GetType().Name}: {nameof(RowNumber)} = {RowNumber}, {nameof(Id)} = {Id}";
    }

    #region Values Extended

    [DebuggerDisplay("{Value}")]
    public partial class StringValue
    {
        public StringValue() { }
        public StringValue(string value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Value?.Equals(obj) ?? false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(StringValue value) => value.Value;
        public static implicit operator StringValue(string value) => new StringValue(value);
    }

    [DebuggerDisplay("{Value}")]
    public partial class BooleanValue
    {
        public BooleanValue() { }
        public BooleanValue(bool? value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator bool? (BooleanValue value) => value.Value;
        public static implicit operator BooleanValue(bool? value) => new BooleanValue(value);
    }

    [DebuggerDisplay("{Value}")]
    public partial class DateTimeValue
    {
        public DateTimeValue() { }
        public DateTimeValue(DateTime? value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator DateTime? (DateTimeValue value) => value.Value;
        public static implicit operator DateTimeValue(DateTime? value) => new DateTimeValue(value);
    }

    [DebuggerDisplay("{Value}")]
    public partial class IntValue
    {
        public IntValue() { }
        public IntValue(int? value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator int? (IntValue value) => value.Value;
        public static implicit operator IntValue(int? value) => new IntValue(value);
    }

    [DebuggerDisplay("{Value}")]
    public partial class LongValue
    {
        public LongValue() { }
        public LongValue(long? value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator long? (LongValue value) => value.Value;
        public static implicit operator LongValue(long? value) => new LongValue(value);
    }

    [DebuggerDisplay("{Value}")]
    public partial class DecimalValue
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

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator decimal? (DecimalValue value) => (decimal)value.Value;
        public static implicit operator DecimalValue(decimal? value) => new DecimalValue(value);
    }

    [DebuggerDisplay("{Value}")]
    public partial class DoubleValue
    {
        public DoubleValue() { }
        public DoubleValue(double? value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator double? (DoubleValue value) => value.Value;
        public static implicit operator DoubleValue(double? value) => new DoubleValue(value);
    }

    [DebuggerDisplay("{Value}")]
    public partial class ShortValue
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

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator short? (ShortValue value) => (short?)value.Value;
        public static implicit operator ShortValue(short? value) => new ShortValue(value);
    }

    [DebuggerDisplay("{Value}")]
    public partial class ByteValue
    {
        public ByteValue() { }
        public ByteValue(byte? value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator byte? (ByteValue value) => (byte?)value.Value;
        public static implicit operator ByteValue(byte? value) => new ByteValue(value);
    }

    [DebuggerDisplay("{Value}")]
    public partial class GuidValue
    {
        public GuidValue() { }
        public GuidValue(Guid? value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator Guid? (GuidValue value) => value.Value;
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
            return Value?.Equals(obj) ?? false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(CustomStringField value) => value.Value;
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
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator bool? (CustomBooleanField value) => value.Value;
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
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator DateTime? (CustomDateTimeField value) => value.Value;
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
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator int? (CustomIntField value) => value.Value;
        public static implicit operator CustomIntField(int? value) => new CustomIntField(value);
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
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator long? (CustomLongField value) => value.Value;
        public static implicit operator CustomLongField(long? value) => new CustomLongField(value);
    }

    [DebuggerDisplay("{Value}")]
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

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator decimal? (CustomDecimalField value) => (decimal)value.Value;
        public static implicit operator CustomDecimalField(decimal? value) => new CustomDecimalField(value);
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
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator double? (CustomDoubleField value) => value.Value;
        public static implicit operator CustomDoubleField(double? value) => new CustomDoubleField(value);
    }

    [DebuggerDisplay("{Value}")]
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

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
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
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator byte? (CustomByteField value) => (byte?)value.Value;
        public static implicit operator CustomByteField(byte? value) => new CustomByteField(value);
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
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator Guid? (CustomGuidField value) => value.Value;
        public static implicit operator CustomGuidField(Guid? value) => new CustomGuidField(value);
    }

    #endregion
}
