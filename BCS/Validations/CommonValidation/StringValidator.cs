using FluentValidation;

namespace HMTSolution.BCS.Validations.CommonValidation
{
    public class StringValidator : BaseStructValidator<string>
    {
        public StringValidator()
        {
            RuleFor(r => r)
                .NotNull().WithName((p) => MessageResult(p))
                .NotEmpty()
                ;
        }

        private string MessageResult(string propertyName)
        {
            return PropertyName ?? propertyName;
        }
    }
}
