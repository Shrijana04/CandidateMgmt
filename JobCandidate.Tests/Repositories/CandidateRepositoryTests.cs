using JobCandidate.HubAPI.Entities;
using JobCandidate.HubAPI.Repositories;
using JobCandidate.HubAPI;
using Microsoft.EntityFrameworkCore;
namespace JobCandidate.Tests.Repositories
{
    public class CandidateRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CandidateRepository _repository;
        public CandidateRepositoryTests()
        {
            // Initialize DbContext with InMemoryDatabase
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed shared data
            _context.Candidates.AddRange(new List<Candidate>
        {
            new Candidate
            {
                Id = new Guid("0b6b10ad-48e5-4ba6-af6a-2b5564130ce0"),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890",
                CallTimeInterval = "9 AM - 5 PM",
                Comments = "Full stack developer",
                GitHubProfileUrl = "https://github.com/johndoe",
                LinkedInProfileUrl = "https://linkedin.com/in/johndoe"
            },
            new Candidate
            {
                Id = new Guid("fffc1323-7d7a-419d-b0da-77d5f424f89a"),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "0987654321",
                CallTimeInterval = "10 AM - 4 PM",
                Comments = "Experienced in software",
                GitHubProfileUrl = "https://github.com/janesmith",
                LinkedInProfileUrl = "https://linkedin.com/in/janesmith"
            }
        });
            _context.SaveChanges();

            // Initialize the repository
            _repository = new CandidateRepository(_context);
        }

        #region Get All
        [Fact]
        public async Task GetAllAsync_ReturnsAllCandidates()
        {

            // Act
            var result = await _repository.GetAllAsync();
            // Assert
            Assert.Equal(2, result.totalCount);
            Assert.Equal(2, result.candidates.Count());
            Assert.Equal("John", result.candidates.First().FirstName);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyResults_WhenNoMatch()
        {
            // Act
            var result = await _repository.GetAllAsync(searchText: "Jane");

            // Assert
            Assert.Single(result.candidates);
            Assert.Equal("Jane", result.candidates.First().FirstName);
        }
        #endregion
        [Fact]
        public async Task AddAsync_AddsCandidateSuccessfully()
        {
            // Arrange
            var candidate = new Candidate
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                PhoneNumber = "5551234567",
                CallTimeInterval = "9 AM - 5 PM",
                Comments = "Excellent skills in .NET development",
            };

            // Act
            await _repository.AddAsync(candidate);

            // Assert
            var savedCandidate = await _context.Candidates.FirstOrDefaultAsync(c => c.Email == "alice.johnson@example.com");

            Assert.NotNull(savedCandidate);
            Assert.Equal("Alice", savedCandidate.FirstName);
            Assert.Equal("Johnson", savedCandidate.LastName);
            Assert.Equal("alice.johnson@example.com", savedCandidate.Email);
            Assert.Equal("5551234567", savedCandidate.PhoneNumber);
            Assert.Equal("9 AM - 5 PM", savedCandidate.CallTimeInterval);
            Assert.Equal("Excellent skills in .NET development", savedCandidate.Comments);
            Assert.Equal("https://github.com/alicejohnson", savedCandidate.GitHubProfileUrl);
            Assert.Equal("https://linkedin.com/in/alicejohnson", savedCandidate.LinkedInProfileUrl);
        }
        [Fact]
        public async Task UpdateAsync_UpdatesCandidateSuccessfully()
        {
            // Arrange
            var candidate = new Candidate
            {
                Id = new Guid("4243d7b5-4223-49f7-9734-a87a2c23bf11"),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "5551234567",
                CallTimeInterval = "9 AM - 5 PM",
                Comments = "Experienced backend developer",
                GitHubProfileUrl = "https://github.com/johndoe",
                LinkedInProfileUrl = "https://linkedin.com/in/johndoe"
            };

            await _context.Candidates.AddAsync(candidate);
            await _context.SaveChangesAsync();

            // Act
            candidate.FirstName = "Jonathan"; // Update the name
            await _repository.UpdateAsync(candidate);

            // Assert
            var updatedCandidate = await _context.Candidates.FindAsync(candidate.Id);
            Assert.NotNull(updatedCandidate);
            Assert.Equal("Jonathan", updatedCandidate.FirstName);
        }
        #region GetById
        [Fact]
        public async Task GetByIdAsync_ReturnsCandidate_WhenCandidateExists()
        {
            // Arrange
            var candidate = new Candidate
            {
                Id = new Guid("8eb75e1e-f487-4baf-9be7-fabd8d8c2a91"),
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                PhoneNumber = "5551234567",
                CallTimeInterval = "9 AM - 5 PM",
                Comments = "Excellent skills in .NET development",
                GitHubProfileUrl = "https://github.com/alicejohnson",
                LinkedInProfileUrl = "https://linkedin.com/in/alicejohnson"
            };

            await _context.Candidates.AddAsync(candidate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(candidate.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(candidate.Id, result.Id);
            Assert.Equal(candidate.FirstName, result.FirstName);
            Assert.Equal(candidate.LastName, result.LastName);
            Assert.Equal(candidate.Email, result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCandidateDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }
        #endregion
        #region Delete
        [Fact]
        public async Task DeleteAsync_RemovesCandidate_WhenCandidateExists()
        {
            // Arrange
            var candidate = new Candidate
            {
                Id = new Guid("6836c720-a9de-46d7-90f9-9820f47fa35b"),
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                PhoneNumber = "5551234567",
                CallTimeInterval = "9 AM - 5 PM",
                Comments = "Excellent skills in .NET development",
                GitHubProfileUrl = "https://github.com/alicejohnson",
                LinkedInProfileUrl = "https://linkedin.com/in/alicejohnson"
            };

            await _context.Candidates.AddAsync(candidate);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(candidate.Id);

            // Assert
            var deletedCandidate = await _context.Candidates.FindAsync(candidate.Id);
            Assert.Null(deletedCandidate);
        }

        #endregion
        #region GetByEmail
        [Fact]
        public async Task GetCandidateByEmail_ReturnsCandidate_WhenCandidateExists()
        {
            // Arrange
            var candidate = new Candidate
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                PhoneNumber = "5551234567",
                CallTimeInterval = "9 AM - 5 PM",
                Comments = "Excellent skills in .NET development",
                GitHubProfileUrl = "https://github.com/alicejohnson",
                LinkedInProfileUrl = "https://linkedin.com/in/alicejohnson"
            };

            await _context.Candidates.AddAsync(candidate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCandidateByEmail(candidate.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(candidate.Id, result.Id);
            Assert.Equal(candidate.FirstName, result.FirstName);
            Assert.Equal(candidate.LastName, result.LastName);
            Assert.Equal(candidate.Email, result.Email);
        }

        [Fact]
        public async Task GetCandidateByEmail_ReturnsNull_WhenCandidateDoesNotExist()
        {
            // Arrange
            var nonExistentEmail = "nonexistent@example.com";

            // Act
            var result = await _repository.GetCandidateByEmail(nonExistentEmail);

            // Assert
            Assert.Null(result);
        }
        #endregion
        public void Dispose()
        {
            // Dispose the DbContext after all tests
            _context.Dispose();
        }
    }

}
