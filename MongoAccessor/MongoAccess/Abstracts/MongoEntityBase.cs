using System;
using System.Collections.Generic;
using System.Text;
using HMTSolution.MongoAccess.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace HMTSolution.MongoAccess.Abstracts
{
    public abstract class MongoEntityBase : IEntity<int>
    {
        protected MongoEntityBase()
        {
        }

        //[BsonRepresentation(BsonType.ObjectId)]
        [BsonId]
        [BsonElement(Order = 0)]
        public int Id { get; set; }// = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement(Order = 101)]
        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
    }
}
