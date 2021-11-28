using Endjin.Core.Retry.Policies;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.MongoRepo.Strategies
{
    public class WriteRetryPolicy : IRetryPolicy
    {
        public bool CanRetry(Exception exception)
        {
            var storageException = exception as MongoWriteException;

            if (storageException == null)
                return true;

            return false;
        }
    }
}
