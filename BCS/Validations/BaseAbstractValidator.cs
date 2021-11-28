using FluentValidation;
using FluentValidation.Results;

namespace HMTSolution.BCS.Validations
{
    public abstract class BaseAbstractValidator<T> : AbstractValidator<T>
    {
        protected override bool PreValidate(ValidationContext<T> context, ValidationResult result)
            => PreValidations.NotNullPreValidation(context, result);
    }
}
