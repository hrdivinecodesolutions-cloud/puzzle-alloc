using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using puzzle_alloc.Data;
using puzzle_alloc.Models.Entities;
using puzzle_alloc.Models.DTOs;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;


namespace puzzle_alloc.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SummaryController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public SummaryController(AppDbContext ctx) { _ctx = ctx; }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<AllocationRunResponse>> Get()
        {
            var containers = await _ctx.Containers.OrderBy(c => c.Index).ToListAsync();
            var containerDtos = containers.Select(c =>
                new ContainerDto(c.Id, c.Index, c.CurrentLoad, c.IsGhost, c.IsActive)).ToList();

            var role = User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value;
            IQueryable<Item> itemsQ = _ctx.Items;


            if (role == "User")
            {
                var uid =
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (string.IsNullOrEmpty(uid))
                    return Unauthorized("Subject (user id) not found in token.");

                itemsQ = itemsQ.Where(i => i.SubmittedByUserId == uid);
            }


            var items = await itemsQ.ToListAsync();
            var allocated = items.Where(i => i.Status == ItemStatus.Allocated).ToList();
            var holding = items.Where(i => i.Status == ItemStatus.Holding).ToList();

        
            var allocations = await _ctx.Allocations
                .Include(a => a.Container)
                .Where(a => allocated.Select(x => x.Id).Contains(a.ItemId))
                .ToListAsync();

            var map = allocations
                .Select(a => new AllocationMapDto(a.ItemId, a.Container.Index))
                .ToList();

            var ghostIds = containerDtos.Where(c => c.IsGhost).Select(c => c.Id).ToList();

            return new AllocationRunResponse(
                allocated.Select(i => new ItemDto(i.Id, i.Name, i.Weight, i.Category, i.Status.ToString())).ToList(),
                holding.Select(i => new ItemDto(i.Id, i.Name, i.Weight, i.Category, i.Status.ToString())).ToList(),
                ghostIds,
                containerDtos,
                map
            );
        }

    }
}
