using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using puzzle_alloc.Services;
using puzzle_alloc.Models.DTOs;

namespace puzzle_alloc.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AllocationController : ControllerBase
    {
        private readonly IAllocationEngine _engine;

        public AllocationController(IAllocationEngine engine) { _engine = engine; }

        [HttpPost("run")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<AllocationRunResponse>> Run()
        {
            var result = await _engine.RunAsync();
            return Ok(result);
        }

    }

}
