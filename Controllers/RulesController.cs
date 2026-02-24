using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using puzzle_alloc.Data;
using puzzle_alloc.Models.DTOs;
using puzzle_alloc.Models.Entities;

namespace puzzle_alloc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RulesController : ControllerBase
    {
        private readonly AppDbContext _ctx;

        public RulesController(AppDbContext ctx) { _ctx = ctx; }

       
        [HttpGet]
        [Authorize] 
        [Produces("application/json")]
        public async Task<ActionResult<RuleSetDto>> GetActive()
        {
            var r = await _ctx.RuleSets.AsNoTracking().FirstOrDefaultAsync(x => x.Active);
            if (r == null) return NotFound();

            return new RuleSetDto(
                r.Id,
                r.MaxCapacityPerContainer,
                r.EnforceCategorySeparation,
                r.GhostContainerCount,
                r.Active
            );
        }

       
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<RuleSetDto>> Create([FromBody] CreateRuleSetDto dto)
        {
            
            var actives = await _ctx.RuleSets.Where(r => r.Active).ToListAsync();
            foreach (var a in actives) a.Active = false;

      
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("Subject (user id) was not present in the token.");

            var r = new RuleSet
            {
                MaxCapacityPerContainer = dto.MaxCapacityPerContainer,
                EnforceCategorySeparation = dto.EnforceCategorySeparation,
                GhostContainerCount = dto.GhostContainerCount,
                Active = dto.Active,
                CreatedByUserId = userId
            };
            _ctx.RuleSets.Add(r);

        
            var containers = await _ctx.Containers.OrderBy(c => c.Index).ToListAsync();
            if (containers.Count == 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    _ctx.Containers.Add(new Container
                    {
                        Index = i,
                        CurrentLoad = 0,
                        IsGhost = i <= r.GhostContainerCount,
                        IsActive = false
                    });
                }
            }
            else
            {
                for (int i = 0; i < containers.Count; i++)
                {
                    containers[i].IsGhost = (i + 1) <= r.GhostContainerCount;
                }
            }

            await _ctx.SaveChangesAsync();

            var response = new RuleSetDto(
                r.Id,
                r.MaxCapacityPerContainer,
                r.EnforceCategorySeparation,
                r.GhostContainerCount,
                r.Active
            );

      
            return CreatedAtAction(nameof(GetActive), new { id = r.Id }, response);
        }
    }
}