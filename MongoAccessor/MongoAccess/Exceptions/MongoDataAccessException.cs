using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.MongoAccess.Exceptions
{
    public class MongoDataAccessException : ApplicationException
    {
        public MongoDataAccessException(string message) : base(message)
        {
        }
    }
}
