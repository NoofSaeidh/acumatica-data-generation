using DataGeneration.Core;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    public static class ValidateHelper
    {
        public static void ValidateObject(object instance)
        {
            Validator.ValidateObject(instance, new ValidationContext(instance));
        }
    }

    public class RequiredCollectionAttribute : RequiredAttribute
    {
        public bool AllowEmpty { get; set; }

        public override bool IsValid(object value)
        {
            if (value == null)
                return false;
            if (value is IEnumerable enumerable)
            {
                if (AllowEmpty)
                    return true;

                return !enumerable.IsNullOrEmpty();
            }
            return false;
        }
    }

    public class ConditionRequiredAttribute : RequiredAttribute
    {
        public ConditionRequiredAttribute(string boolPropertyName)
        {
            BoolPropertyNameToCheck = boolPropertyName ?? throw new ArgumentNullException(nameof(boolPropertyName));
        }

        public string BoolPropertyNameToCheck { get; set; }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(BoolPropertyNameToCheck);
            if (property == null)
                return new ValidationResult($"Cannot find property: {BoolPropertyNameToCheck}", BoolPropertyNameToCheck.AsArray());
            if (property.PropertyType != typeof(bool))
                return new ValidationResult($"Property {BoolPropertyNameToCheck} must be of Boolean type.", BoolPropertyNameToCheck.AsArray());

            try
            {
                var propValue = (bool)property.GetValue(validationContext.ObjectInstance, null);
                if(!propValue)
                    return ValidationResult.Success;
            }
            catch
            {
                return new ValidationResult($"Cannot get value of property: {BoolPropertyNameToCheck}", BoolPropertyNameToCheck.AsArray());
            }

            return base.IsValid(value, validationContext);
        }
    }
}