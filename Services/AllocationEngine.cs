namespace puzzle_alloc.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using puzzle_alloc.Data;
    using puzzle_alloc.Models.DTOs;
    using puzzle_alloc.Models.Entities;

    public class AllocationEngine : IAllocationEngine
    {
        private readonly AppDbContext _ctx;
        public AllocationEngine(AppDbContext ctx) { _ctx = ctx; }
        public async Task<AllocationRunResponse> RunAsync()
        {
           
            var rules = await _ctx.RuleSets.AsNoTracking().FirstOrDefaultAsync(r => r.Active);
            if (rules == null) throw new InvalidOperationException("No active rules found.");

            var containers = await _ctx.Containers.OrderBy(c => c.Index).ToListAsync();
            var items = await _ctx.Items.Where(i => i.Status != ItemStatus.Allocated)
                                        .OrderBy(i => i.Id).ToListAsync();

            var lastByCategory = new Dictionary<string, int>();
            var allocated = new List<Item>();
            var holding = new List<Item>();
            var map = new List<AllocationMapDto>();

            foreach (var item in items)
            {
                bool placed = false;

                foreach (var c in containers)
                {
                    // capacity
                    if (c.CurrentLoad + item.Weight > rules.MaxCapacityPerContainer) continue;

                    // category separation (no consecutive containers)
                    if (rules.EnforceCategorySeparation &&
                        lastByCategory.TryGetValue(item.Category, out var lastIdx) &&
                        c.Index == lastIdx + 1)
                    {
                        continue;
                    }

                    // place
                    c.CurrentLoad += item.Weight;
                    c.IsActive = true;
                    if (c.IsGhost) c.IsGhost = false;

                    _ctx.Allocations.Add(new Allocation { ItemId = item.Id, ContainerId = c.Id });
                    item.Status = ItemStatus.Allocated;

                    allocated.Add(item);
                    lastByCategory[item.Category] = c.Index;
                    map.Add(new AllocationMapDto(item.Id, c.Index));

                    placed = true;
                    break;
                }

                if (!placed)
                {
                    item.Status = ItemStatus.Holding;
                    holding.Add(item);
                }
            }

            await _ctx.SaveChangesAsync();

            var containerDtos = containers.Select(c => new ContainerDto(c.Id, c.Index, c.CurrentLoad, c.IsGhost, c.IsActive)).ToList();
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