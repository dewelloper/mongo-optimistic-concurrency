using FluentValidation;
using FluentValidation.Results;

namespace HMTSolution.BCS.Validations
{
    public static class PreValidations
    {
        public static bool NotNullPreValidation<T>(ValidationContext<T> context, ValidationResult result)
        {
            var shouldContinue = true;

            if ((typeof(T).IsValueType || typeof(T) == typeof(string)) == false && context.InstanceToValidate == null)
            {
                shouldContinue = false;
                result.Errors.Add(new ValidationFailure(typeof(T).Name, "Object should not be null"));
            }

            return shouldContinue;
        }
    }
}
