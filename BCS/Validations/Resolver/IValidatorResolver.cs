using System;
using FluentValidation;

namespace HMTSolution.BCS.Validations.Resolver
{
    public interface IValidatorResolver
    {
        T Resolve<T>(string propertyName, bool throwIfTypeNotFound = true) where T : class;
        IValidator Resolve(Type type, bool throwIfTypeNotFound = true);
    }
}
