namespace puzzle_alloc.Models.Entities
{

    public class RuleSet
    {
        public int Id { get; set; }
        public int MaxCapacityPerContainer { get; set; }
        public bool EnforceCategorySeparation { get; set; }
        public int GhostContainerCount { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByUserId { get; set; } = default!;
    }

}
