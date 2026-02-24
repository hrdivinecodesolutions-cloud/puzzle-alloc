namespace puzzle_alloc.Models.DTOs
{

    public record SubmitItemDto(string Name, decimal Weight, string Category);
    public record ItemDto(int Id, string Name, decimal Weight, string Category, string Status);

}
