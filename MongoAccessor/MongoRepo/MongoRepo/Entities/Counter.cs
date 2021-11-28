using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.MongoRepo.Entities
{
    public class Counter
    {
        Counter()
        {
        }

        public string? Id { get; set; }
        public int Value { get; set; }
    }
}
