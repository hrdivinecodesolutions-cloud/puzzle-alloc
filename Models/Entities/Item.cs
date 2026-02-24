namespace puzzle_alloc.Models.Entities
{

    public enum ItemStatus { Submitted = 0, Allocated = 1, Holding = 2 }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Weight { get; set; }
        public string Category { get; set; } = default!;
        public string SubmittedByUserId { get; set; } = default!;
        public ItemStatus Status { get; set; } = ItemStatus.Submitted;
    }

}
