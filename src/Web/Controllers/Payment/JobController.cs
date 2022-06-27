using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Application.Commands;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : BaseController
    {
        private readonly ILogger<JobController> _logger;

        public JobController(ILogger<JobController> logger)
        {
            _logger = logger;
        }


        //[HttpGet("ProcessUncapturedPayments")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> ProcessUncapturedPayments(int daysAgo = 1)
        //{
        //    try
        //    {
        //        var result = await Mediator.Send(new ProcessUncapturedPaymentsCommand(daysAgo));

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unable to process uncaptured payments");

        //        return StatusCode(StatusCodes.Status500InternalServerError);
        //    }
        //}

        [HttpGet("ProcessUncapturedRefunds")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessUncapturedRefunds(int daysAgo = 1)
        {
            try
            {
                var result = await Mediator.Send(new ProcessUncapturedRefundsCommand(daysAgo));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process and confirm refund payments");

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
