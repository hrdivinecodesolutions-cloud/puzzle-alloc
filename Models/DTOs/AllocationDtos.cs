using puzzle_alloc.Models.Entities;

namespace puzzle_alloc.Models.DTOs
{

    public record AllocationMapDto(int ItemId, int ContainerIndex);

    public record AllocationRunResponse(
        List<ItemDto> Allocated,
        List<ItemDto> Holding,
        List<int> GhostContainerIds,
        List<ContainerDto> Containers,
        List<AllocationMapDto> Map
    );

}
