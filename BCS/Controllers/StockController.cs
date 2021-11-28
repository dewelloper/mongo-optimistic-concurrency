using HMTSolution.BCS.Validations.Resolver;
using HMTSolution.MongoRepo.Entities;
using HMTSolution.MongoRepo.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMTSolution.BCS.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class StockController : BaseController
    {
        private readonly IStockRepository _stockRepository;

        public StockController(ILogger<StockController> logger, IValidatorResolver validator, IStockRepository stockRepository)
            : base(logger, validator)
        {
            _stockRepository = stockRepository;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var result = _stockRepository.Get();
                if (result == null)
                {
                    return BadRequest("Not found");
                }

                return Ok(result.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(error: ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _stockRepository.GetByIdAsync(id);
                if (result == null)
                {
                    return BadRequest("Not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(error: ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] StockEntity data)
        {
            try
            {
                await _stockRepository.AddAsync(data, true);
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(error: ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StockEntity stockEntity)
        {
            try
            {
                stockEntity.Id = id;
                var isExist = await _stockRepository.GetByIdAsync(id);
                if (isExist == null)
                {
                    // explanation for await usage 
                    // https://docs.microsoft.com/en-us/archive/blogs/vancem/diagnosing-net-core-threadpool-starvation-with-perfview-why-my-service-is-not-saturating-all-cores-or-seems-to-stall?WT.mc_id=DT-MVP-5003493
                    // Dont forget! In normally 1 thread take to background or forground (realise-time) takes 6 QA! But in MongoDb it takes 50 ms. count with that
                    await _stockRepository.AddAsync(stockEntity, true);
                    return Ok();
                }

                stockEntity.Quantity = (int)isExist.Quantity + 1;
                await _stockRepository.UpdateAsync(id, stockEntity, true, false);

                return Ok(stockEntity);
            }
            catch (Exception ex)
            {
                return BadRequest(error: ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _stockRepository.DeleteAsync(id);
                if (result == null)
                {
                    return BadRequest("Not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(error: ex.Message);
            }
        }
    }

}
