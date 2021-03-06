using Endjin.Core.Retry.Strategies;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.MongoRepo.Strategies
{
    public class WriteRetryStrategy : Linear
    {
        private const int MaxTries = 5;
        private static readonly TimeSpan Periodicity = new TimeSpan(0, 0, 2);

        private WriteRetryStrategy(TimeSpan periodicity, int maxTries) : base(periodicity, maxTries)
        {
        }

        public static WriteRetryStrategy Create()
        {
            return new WriteRetryStrategy(Periodicity, MaxTries);
        }

        public static WriteRetryStrategy Create(TimeSpan periodicity, int maxTries)
        {
            return new WriteRetryStrategy(periodicity, maxTries);
        }
    }
}
