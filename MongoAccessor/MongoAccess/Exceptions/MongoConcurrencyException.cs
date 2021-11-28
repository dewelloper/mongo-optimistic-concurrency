using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.MongoAccess.Exceptions
{
    public class MongoConcurrencyException : ApplicationException
    {
        public MongoConcurrencyException(string message) : base(message)
        {
        }
    
    }
}
