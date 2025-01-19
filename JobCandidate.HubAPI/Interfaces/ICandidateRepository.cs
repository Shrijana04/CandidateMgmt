using JobCandidate.HubAPI.Entities;

namespace JobCandidate.HubAPI.Interfaces
{
    public interface ICandidateRepository
    {
        Task<(IEnumerable<Candidate> candidates, int totalCount)> GetAllAsync(string searchText = "", string sortField = "", bool isDescending = false, int maxResultCount = int.MaxValue, int skipCount = 0);
        Task<Candidate> GetByIdAsync(Guid id); // get Candidate by id
        Task AddAsync(Candidate candidate);     // Add candidate
        Task UpdateAsync(Candidate candidate);  // update candidate
        Task DeleteAsync(Guid id);     // Delete candidate
        Task<Candidate> GetCandidateByEmail(string email); // Get By Email
    }
}
