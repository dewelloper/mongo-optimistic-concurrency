using HMTSolution.MongoAccess.Abstracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.MongoRepo.Entities
{
    public class StockEntity : MongoEntityBase
    {
        public string? VariantCode { get; set; }
        public string? ProductCode { get; set; }
        public DateTime CreateTime { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
