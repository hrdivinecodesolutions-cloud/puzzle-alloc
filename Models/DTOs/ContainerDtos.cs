namespace puzzle_alloc.Models.DTOs
{

   public record ContainerDto(
        int Id,
        int Index,
        decimal CurrentLoad,
        bool IsGhost,
        bool IsActive
    );

}
