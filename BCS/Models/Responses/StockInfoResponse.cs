using System;

namespace HMTSolution.BCS.Models.Response
{
    public class StockInfoResponse
    {
        public string Id { get; set; }
        public string VariantCode { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

}
