using Application.Commands;
using Application.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefundController : BaseController
    {
        private readonly ILogger<RefundController> _logger;
        private readonly IMapper _mapper;

        public RefundController(
            ILogger<RefundController> logger,
            IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Post(RefundModel model)
        {
            try
            {
                var result = await Mediator.Send(new RefundRequestCommand()
                {
                    Refund = _mapper.Map<Refund>(model)
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process the refund");

                return BadRequest();
            }
        }
    }
}
