using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.MongoAccess.Interfaces
{
    public interface IEntity<out TKey> : IEntityBase where TKey : IEquatable<TKey>
    {
        public TKey Id { get; }
        DateTime UpdateTime { get; set; }
    }
}