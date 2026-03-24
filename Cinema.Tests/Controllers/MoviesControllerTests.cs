using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cinema.API.Controllers;
using Cinema.API.DTOs;
using Cinema.Core.Entities;
using Cinema.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Cinema.Tests.Controllers
{
    public class MoviesControllerTests
    {
        private readonly Mock<IMovieRepository> _mockRepo;
        private readonly MoviesController _controller;

        public MoviesControllerTests()
        {
            _mockRepo = new Mock<IMovieRepository>();
            _controller = new MoviesController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfMovies()
        {
            // Arrange
            var mockMovies = new List<Movie>
            {
                new Movie { Id = 1, Title = "Movie 1", Genre = "Action" },
                new Movie { Id = 2, Title = "Movie 2", Genre = "Comedy" }
            };
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(mockMovies);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnMovies = Assert.IsAssignableFrom<IEnumerable<Movie>>(okResult.Value);
            Assert.Equal(2, ((List<Movie>)returnMovies).Count);
        }

        [Fact]
        public async Task GetById_WhenMovieExists_ReturnsOkResult()
        {
            // Arrange
            int movieId = 1;
            var mockMovie = new Movie { Id = movieId, Title = "Movie 1" };
            _mockRepo.Setup(repo => repo.GetByIdAsync(movieId)).ReturnsAsync(mockMovie);

            // Act
            var result = await _controller.GetById(movieId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultMovie = Assert.IsAssignableFrom<Movie>(okResult.Value);
            Assert.Equal(movieId, resultMovie.Id);
        }

        [Fact]
        public async Task GetById_WhenMovieDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            int movieId = 99;
            _mockRepo.Setup(repo => repo.GetByIdAsync(movieId)).ReturnsAsync((Movie?)null);

            // Act
            var result = await _controller.GetById(movieId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Create_ValidObjectPassed_ReturnsCreatedResponse()
        {
            // Arrange
            var movieRequest = new MovieRequest
            {
                Title = "New Movie",
                Description = "A new description",
                DurationInMinutes = 120,
                ReleaseDate = DateTime.Now,
                Genre = "Sci-Fi"
            };

            // Act
            var result = await _controller.Create(movieRequest);

            // Assert
            var createdResponse = Assert.IsType<CreatedAtActionResult>(result);
            var createdMovie = Assert.IsAssignableFrom<Movie>(createdResponse.Value);
            
            Assert.Equal(movieRequest.Title, createdMovie.Title);
            Assert.Equal("GetById", createdResponse.ActionName);
        }

        [Fact]
        public async Task Update_WhenMovieExists_ReturnsOkResultAndUpdates()
        {
            // Arrange
            int movieId = 1;
            var existingMovie = new Movie { Id = movieId, Title = "Old Title" };
            var updateRequest = new MovieRequest { Title = "Updated Title", Genre = "Action" };

            _mockRepo.Setup(repo => repo.GetByIdAsync(movieId)).ReturnsAsync(existingMovie);

            // Act
            var result = await _controller.Update(movieId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedMovie = Assert.IsAssignableFrom<Movie>(okResult.Value);
            
            Assert.Equal("Updated Title", updatedMovie.Title);
            Assert.Equal("Action", updatedMovie.Genre);
            _mockRepo.Verify(r => r.UpdateAsync(existingMovie), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenMovieExists_ReturnsNoContentResult()
        {
            // Arrange
            int movieId = 1;
            var existingMovie = new Movie { Id = movieId, Title = "Movie 1" };
            _mockRepo.Setup(repo => repo.GetByIdAsync(movieId)).ReturnsAsync(existingMovie);

            // Act
            var result = await _controller.Delete(movieId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepo.Verify(r => r.DeleteAsync(movieId), Times.Once);
        }
    }
}
