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
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public ItemsController(AppDbContext ctx) { _ctx = ctx; }

        /// <summary>
        /// Submit a new item (Admin or User).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<ItemDto>> Submit([FromBody] SubmitItemDto dto)
        {
         
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("Subject (user id) was not present in the token.");

            var item = new Item
            {
                Name = dto.Name,
                Weight = dto.Weight,
                Category = dto.Category,
                SubmittedByUserId = userId,
                Status = ItemStatus.Submitted
            };

            _ctx.Items.Add(item);
            await _ctx.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMy), new { id = item.Id },
                new ItemDto(item.Id, item.Name, item.Weight, item.Category, item.Status.ToString()));
        }

        /// <summary>
        /// Get items submitted by the current user.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Admin,User")]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetMy()
        {
            var uid =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(uid))
                return Unauthorized("Subject (user id) was not present in the token.");

            var items = await _ctx.Items
                .Where(i => i.SubmittedByUserId == uid)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            return items
                .Select(i => new ItemDto(i.Id, i.Name, i.Weight, i.Category, i.Status.ToString()))
                .ToList();
        }

        /// <summary>
        /// Get all items 
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll()
        {
            var items = await _ctx.Items
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            return items
                .Select(i => new ItemDto(i.Id, i.Name, i.Weight, i.Category, i.Status.ToString()))
                .ToList();
        }
    }
}