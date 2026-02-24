namespace puzzle_alloc.Models.Entities
{

    public class Container
    {
        public int Id { get; set; }
        public int Index { get; set; }           
        public decimal CurrentLoad { get; set; } 
        public bool IsGhost { get; set; }        
        public bool IsActive { get; set; }       
    }

}
