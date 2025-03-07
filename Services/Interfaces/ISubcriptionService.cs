using BusinessObjects.Base;
using DTOs.DishDTOs;
using DTOs.SubcriptionDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ISubcriptionService
    {
        Task<IEnumerable<SubcriptionResponse>> GetAllSubcriptionAsync();
        Task<SubcriptionResponse> GetSubcriptionByIdAsync(string Id);
        Task CreateSubcriptionAsync(SubcriptionRequest request);
        Task UpdateSubcriptionAsync(string Id, SubcriptionRequest request);
        Task DeleteSubcriptionAsync(string Id);
    }
}
