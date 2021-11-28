using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.MongoRepo.Entities
{
    //This class will also do the job of giving incremental ids. But it will also log the requests received 
    //in the live environment with the optimistic cncurrency and the exception performances it is for feature observation.
    class MongoCollectionCounter
    {
        private const string CounterCollectionName = "StockCounters";
        private readonly string _counterName;
        private readonly MongoClient _client;
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<Counter> _collection;

        public MongoCollectionCounter(string counterName, MongoClient client, IMongoDatabase db)
        {
            _counterName = counterName;
            _client = client;
            _db = db;
            _collection = db.GetCollection<Counter>(CounterCollectionName);
        }
        public int GetNextNumber()
        {
            var filter = Builders<Counter>.Filter.Eq("_id", _counterName);
            var update = Builders<Counter>.Update.Inc(c => c.Value, 1);
            var options = new FindOneAndUpdateOptions<Counter> { IsUpsert = true, ReturnDocument = ReturnDocument.After };
            var result = _collection.FindOneAndUpdate(filter, update, options);

            return result.Value;
        }
    }



}
