using AutoMapper;
using JobCandidate.HubAPI.Controllers;
using JobCandidate.HubAPI.Entities;
using JobCandidate.HubAPI.Interfaces;
using JobCandidate.HubAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace JobCandidate.Tests.Controllers
{
    public class CandidatesControllerTests
    {
        private readonly Mock<ICandidateRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CandidateController _controller;

        public CandidatesControllerTests()
        {
            _mockRepository = new Mock<ICandidateRepository>();
            _mockMapper = new Mock<IMapper>();
            _controller = new CandidateController(_mockRepository.Object, _mockMapper.Object);
        }
        #region GetAll
        [Fact]
        public async Task GetAll_ReturnsOkResult_WithMappedCandidates()
        {
            // Arrange
            var input = new GetCandidateInput
            {
                MaxResultCount = 5,
                SkipCount = 0
            };

            var mockCandidates = new List<Candidate>
        {
            new Candidate { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" },
            new Candidate { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com" }
        };

            var mockCandidateModels = new List<CandidateModel>
        {
            new CandidateModel { FirstName = "John", LastName="Doe", Email = "john.doe@example.com" },
            new CandidateModel { FirstName = "Jane", LastName= "Smith", Email = "jane.smith@example.com" }
        };

            var mockResult = (mockCandidates, totalCount: 2);

            _mockRepository
                .Setup(repo => repo.GetAllAsync(input.SearchText, input.Sorting, input.IsDescending, input.MaxResultCount, input.SkipCount))
                .ReturnsAsync(mockResult);

            _mockMapper
                .Setup(mapper => mapper.Map<List<CandidateModel>>(mockCandidates))
                .Returns(mockCandidateModels);

            // Act
            var result = await _controller.GetAll(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseData = Assert.IsType<GetAllResponse>(okResult.Value);

            Assert.Equal(2, responseData.TotalCount);
            Assert.Equal("John", responseData.Items[0].FirstName);
            Assert.Equal("Jane", responseData.Items[1].FirstName);

            // Verify repository and mapper calls
            _mockRepository.Verify(repo => repo.GetAllAsync(input.SearchText, input.Sorting, input.IsDescending, input.MaxResultCount, input.SkipCount), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<List<CandidateModel>>(mockCandidates), Times.Once);
        }
        #endregion
        #region GetById
        [Fact]
        public async Task GetById_ReturnsOkResult_WithCandidateModel_WhenCandidateExists()
        {
            // Arrange
            var candidateId = Guid.NewGuid();
            var candidate = new Candidate
            {
                Id = candidateId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            var candidateModel = new CandidateModel
            {
                Id = candidateId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(candidate);

            _mockMapper
                .Setup(mapper => mapper.Map<CandidateModel>(candidate))
                .Returns(candidateModel);

            // Act
            var result = await _controller.GetById(candidateId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedModel = Assert.IsType<CandidateModel>(okResult.Value);

            Assert.Equal(candidateId, returnedModel.Id);
            Assert.Equal("John", returnedModel.FirstName);
            Assert.Equal("john.doe@example.com", returnedModel.Email);

            _mockRepository.Verify(repo => repo.GetByIdAsync(candidateId), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<CandidateModel>(candidate), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WithNull_WhenCandidateDoesNotExist()
        {
            // Arrange
            var candidateId = Guid.NewGuid();
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync((Candidate)null);

            // Act
            var result = await _controller.GetById(candidateId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);

            _mockRepository.Verify(repo => repo.GetByIdAsync(candidateId), Times.Once);
        }
        [Fact]
        public async Task GetById_ReturnsBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var candidateId = Guid.NewGuid();

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(candidateId))
                .ThrowsAsync(new Exception("An error occurred."));

            // Act
            var result = await _controller.GetById(candidateId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("An error occurred.", badRequestResult.Value);

            _mockRepository.Verify(repo => repo.GetByIdAsync(candidateId), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<CandidateModel>(It.IsAny<Candidate>()), Times.Never);
        }
        #endregion
        #region Create Candidate
        [Fact]
        public async Task CreateAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "The Email field is required.");

            var input = new CreateModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = ""
            };

            // Act
            var result = await _controller.CreateAsync(input);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateAsync_ReturnsOk_WhenCandidateIsSuccessfullyCreated()
        {
            // Arrange
            var input = new CreateModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "new.email@example.com"
            };

            _mockRepository
                .Setup(repo => repo.GetCandidateByEmail(input.Email))
                .ReturnsAsync((Candidate)null);

            _mockMapper
                .Setup(mapper => mapper.Map<Candidate>(input))
                .Returns(new Candidate
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "new.email@example.com"
                });

            _mockRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Candidate>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateAsync(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Candidate saved successfully.", okResult.Value);

            _mockRepository.Verify(repo => repo.GetCandidateByEmail(input.Email), Times.Once);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Candidate>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ReturnsBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var input = new CreateModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "new.email@example.com"
            };

            _mockRepository
                .Setup(repo => repo.GetCandidateByEmail(input.Email))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateAsync(input);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Database error", badRequestResult.Value);

            _mockRepository.Verify(repo => repo.GetCandidateByEmail(input.Email), Times.Once);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Candidate>()), Times.Never);
        }

        #endregion
        #region Update Candidate 
        [Fact]
        public async Task UpdateAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "The Email field is required.");

            var input = new CreateModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = ""
            };

            var id = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateAsync(input, id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsBadRequest_WhenEmailAlreadyExistsForDifferentCandidate()
        {
            // Arrange
            var input = new CreateModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing.email@example.com"
            };

            var id = Guid.NewGuid();

            var existingCandidate = new Candidate
            {
                Id = Guid.NewGuid(),
                Email = "existing.email@example.com"
            };

            _mockRepository
                .Setup(repo => repo.GetCandidateByEmail(input.Email))
                .ReturnsAsync(existingCandidate);

            // Act
            var result = await _controller.UpdateAsync(input, id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email address already exist.", badRequestResult.Value);

            _mockRepository.Verify(repo => repo.GetCandidateByEmail(input.Email), Times.Once);
            _mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Candidate>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOk_WhenCandidateIsSuccessfullyUpdated()
        {
            // Arrange
            var input = new CreateModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "updated.email@example.com"
            };

            var id = Guid.NewGuid();

            var existingCandidate = new Candidate
            {
                Id = id,
                FirstName = "OldFirstName",
                LastName = "OldLastName",
                Email = "old.email@example.com"
            };

            _mockRepository
                .Setup(repo => repo.GetCandidateByEmail(input.Email))
                .ReturnsAsync((Candidate)null);

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync(existingCandidate);

            _mockRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<Candidate>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateAsync(input, id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Candidate updated successfully.", okResult.Value);

            _mockRepository.Verify(repo => repo.GetCandidateByEmail(input.Email), Times.Once);
            _mockRepository.Verify(repo => repo.GetByIdAsync(id), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map(input, existingCandidate), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(existingCandidate), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var input = new CreateModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "updated.email@example.com"
            };

            var id = Guid.NewGuid();

            _mockRepository
                .Setup(repo => repo.GetCandidateByEmail(input.Email))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateAsync(input, id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Database error", badRequestResult.Value);

            _mockRepository.Verify(repo => repo.GetCandidateByEmail(input.Email), Times.Once);
            _mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Candidate>()), Times.Never);
        }

        #endregion
        #region Delete Candidate 
        [Fact]
        public async Task DeleteAsync_ReturnsOk_WhenCandidateIsSuccessfullyDeleted()
        {
            // Arrange
            var candidateId = Guid.NewGuid();

            var existingCandidate = new Candidate
            {
                Id = candidateId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(existingCandidate);

            _mockRepository
                .Setup(repo => repo.DeleteAsync(candidateId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAsync(candidateId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Candidate deleted successfully.", okResult.Value);

            _mockRepository.Verify(repo => repo.GetByIdAsync(candidateId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(candidateId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsNotFound_WhenCandidateDoesNotExist()
        {
            // Arrange
            var candidateId = Guid.NewGuid();

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync((Candidate)null);

            // Act
            var result = await _controller.DeleteAsync(candidateId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Candidate not found with the specified id.", notFoundResult.Value);

            _mockRepository.Verify(repo => repo.GetByIdAsync(candidateId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var candidateId = Guid.NewGuid();

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(candidateId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteAsync(candidateId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Database error", badRequestResult.Value);

            _mockRepository.Verify(repo => repo.GetByIdAsync(candidateId), Times.Once);
            _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }
        #endregion
    }
}
