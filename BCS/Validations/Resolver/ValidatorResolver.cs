using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace HMTSolution.BCS.Validations.Resolver
{
    public class ValidatorResolver : IValidatorResolver
    {
        private readonly IEnumerable<IValidator> _validators;

        public ValidatorResolver(IEnumerable<IValidator> validators)
        {
            _validators = validators;
        }

        public T Resolve<T>(string propertyName, bool throwIfTypeNotFound = true) where T : class
        {
            var validator = Resolve(typeof(T), throwIfTypeNotFound);
            if (validator is IStructValidator structValidator)
            {
                structValidator.PropertyName = propertyName;
            }

            return validator as T;
        }

        public IValidator Resolve(Type type, bool throwIfTypeNotFound = true)
        {
            var validator = _validators.FirstOrDefault(v => v.GetType() == type);

            if (throwIfTypeNotFound && validator == null)
            {
                throw new ArgumentNullException($"{type.Name} cannot found");
            }

            return validator;
        }
    }
}
