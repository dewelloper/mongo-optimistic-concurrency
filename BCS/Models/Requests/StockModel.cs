using Microsoft.AspNetCore.Mvc;

namespace HMTSolution.BCS.Models.Requests
{
    public class StockModel
    {
        /// <summary>
        /// Product Code
        /// </summary>
        /// <example>Product Code</example>
        [FromRoute]
        public string ProductCode { get; set; }
    }
}
