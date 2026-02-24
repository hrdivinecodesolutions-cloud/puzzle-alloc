
using System.Threading.Tasks;
using puzzle_alloc.Models.Entities;
using puzzle_alloc.Models.DTOs;


namespace puzzle_alloc.Services
{ 
    public interface IAllocationEngine
    {
        Task<AllocationRunResponse> RunAsync();
    }

}
