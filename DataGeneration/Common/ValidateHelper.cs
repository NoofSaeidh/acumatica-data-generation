using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Common
{
    public static class ValidateHelper
    {
        public static void ValidateObject(object instance)
        {
            var context = new ValidationContext(instance);
            var results = new List<ValidationResult>();
            Validator.ValidateObject(instance, context);
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
}