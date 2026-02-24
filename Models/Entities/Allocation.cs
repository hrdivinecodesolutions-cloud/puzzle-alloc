namespace puzzle_alloc.Models.Entities
{
 public class Allocation
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; } = default!;
        public int ContainerId { get; set; }
        public Container Container { get; set; } = default!;
        public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
    }

}
