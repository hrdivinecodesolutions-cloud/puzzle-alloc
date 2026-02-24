namespace puzzle_alloc.Models.DTOs
{

    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string Token, string Role, string Email);

}
