using FluentValidation;
using HMTSolution.BCS.Models.Requests;

namespace HMTSolution.BCS.Validations.StockValidation
{
    public class StockRequestValidator : BaseAbstractValidator<StockRequest>
    {
        public StockRequestValidator()
        {
            RuleFor(r => r)
                .NotNull()
                .NotEmpty()
                ;

            RuleFor(r => r.ProductCode)
                .NotNull()
                .NotEmpty()
                .Matches("^[a-z0-9.-]{3,200}$").WithMessage("{PropertyName} 'Küçük Harf a..z' , '0-9' , '- (tire)' , '. (nokta)'  karaktelerini içerip en az 3 ile 200 karakter arasında olması gereklidir.")
                ;

        }
    }
}
