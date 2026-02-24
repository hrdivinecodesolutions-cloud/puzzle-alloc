namespace puzzle_alloc.Models.DTOs
{

    public record CreateRuleSetDto(int MaxCapacityPerContainer, bool EnforceCategorySeparation, int GhostContainerCount, bool Active);
    public record RuleSetDto(int Id, int MaxCapacityPerContainer, bool EnforceCategorySeparation, int GhostContainerCount, bool Active);

}
