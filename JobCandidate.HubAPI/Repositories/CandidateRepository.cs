using JobCandidate.HubAPI.Entities;
using JobCandidate.HubAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobCandidate.HubAPI.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly ApplicationDbContext _context;

        public CandidateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Candidate> candidates, int totalCount)> GetAllAsync(string searchText = "", string sortField = "", bool isDescending = false, int maxResultCount = int.MaxValue, int skipCount = 0)
        {
            // Step 1: Filter based on searchText (if provided)
            var query = _context.Candidates.AsQueryable();
            // Step 2: Filter based on searchText
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(c =>
                    (c.FirstName + " " + c.LastName).ToLower().Contains(searchText.Trim().ToLower()) || // Concatenate FirstName and LastName
                    c.Email.ToLower().Contains(searchText.Trim().ToLower()) ||
                    c.PhoneNumber.Contains(searchText));
            }

            // Step 3: Apply sorting
            if (!string.IsNullOrEmpty(sortField))
            {
                query = sortField switch
                {
                    "firstname" => isDescending
                        ? query.OrderByDescending(c => c.FirstName)
                        : query.OrderBy(c => c.FirstName),
                    "lastname" => isDescending
                        ? query.OrderByDescending(c => c.LastName)
                        : query.OrderBy(c => c.LastName),
                    "email" => isDescending
                        ? query.OrderByDescending(c => c.Email)
                        : query.OrderBy(c => c.Email),
                    "phonenumber" => isDescending
                        ? query.OrderByDescending(c => c.PhoneNumber)
                        : query.OrderBy(c => c.PhoneNumber),
                    _ => isDescending
                        ? query.OrderByDescending(c => c.CreationTime)
                        : query.OrderBy(c => c.CreationTime) // Default sorting by Id
                };
            }
            else
            {
                query = query.OrderBy(c => c.CreationTime); // Default sorting by Id
            }

            // Step 3: Get total count before applying pagination
            var totalCount = await query.CountAsync();

            // Step 4: Apply pagination
            var candidates = await query
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();

            // Step 5: Return results
            return (candidates, totalCount);
        }

        public async Task AddAsync(Candidate candidate)
        {
            await _context.Candidates.AddAsync(candidate);
            _context.SaveChanges();
        }

        public async Task UpdateAsync(Candidate candidate)
        {
            _context.Candidates.Update(candidate);
            _context.SaveChanges();
        }

        public async Task<Candidate> GetByIdAsync(Guid id)
        {
            return await _context.Candidates.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var candidate = await GetByIdAsync(id);
            if (candidate != null)
            {
                _context.Candidates.Remove(candidate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Candidate> GetCandidateByEmail(string email)
        {

            return await Task.FromResult(_context.Candidates.SingleOrDefault(x => x.Email == email));
        }
    }
}