using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Cinema.API.Controllers;
using Cinema.API.DTOs;
using Cinema.Core.Entities;
using Cinema.Core.Enums;
using Cinema.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Cinema.Tests.Controllers
{
    public class TicketsControllerTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IShowTimeRepository> _mockShowTimeRepo;
        private readonly TicketsController _controller;

        public TicketsControllerTests()
        {
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockShowTimeRepo = new Mock<IShowTimeRepository>();
            _controller = new TicketsController(_mockTicketRepo.Object, _mockShowTimeRepo.Object);

            // Mocking HttpContext User for claims (UserId = 1)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetMyTickets_ReturnsOkResult_WithUserTickets()
        {
            // Arrange
            var mockTickets = new List<Ticket>
            {
                new Ticket { Id = 1, UserId = 1, ShowTimeId = 1, SeatNumber = 15 }
            };
            _mockTicketRepo.Setup(repo => repo.GetByUserIdAsync(1)).ReturnsAsync(mockTickets);

            // Act
            var result = await _controller.GetMyTickets();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnTickets = Assert.IsAssignableFrom<IEnumerable<Ticket>>(okResult.Value);
            Assert.Single((List<Ticket>)returnTickets);
        }

        [Fact]
        public async Task BookTicket_WhenShowTimeNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new TicketRequest { ShowTimeId = 99, SeatNumber = 10 };
            _mockShowTimeRepo.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((ShowTime?)null);

            // Act
            var result = await _controller.BookTicket(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task BookTicket_WhenNoAvailableSeats_ReturnsBadRequest()
        {
            // Arrange
            var request = new TicketRequest { ShowTimeId = 1, SeatNumber = 10 };
            var mockShowTime = new ShowTime { Id = 1, AvailableSeats = 0 };
            _mockShowTimeRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(mockShowTime);

            // Act
            var result = await _controller.BookTicket(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("No available seats", badRequestResult.Value?.ToString() ?? "");
        }

        [Fact]
        public async Task BookTicket_WithValidData_ReturnsCreatedResponse_AndDecrementsSeats()
        {
            // Arrange
            var request = new TicketRequest { ShowTimeId = 1, SeatNumber = 10 };
            var mockShowTime = new ShowTime { Id = 1, AvailableSeats = 50 };
            
            _mockShowTimeRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(mockShowTime);
            _mockTicketRepo.Setup(repo => repo.AddAsync(It.IsAny<Ticket>())).ReturnsAsync(new Ticket { Id = 1, UserId = 1, ShowTimeId = 1, SeatNumber = 10 });
            _mockShowTimeRepo.Setup(repo => repo.UpdateAsync(It.IsAny<ShowTime>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.BookTicket(request);

            // Assert
            var createdResponse = Assert.IsType<CreatedAtActionResult>(result);
            var createdTicket = Assert.IsAssignableFrom<Ticket>(createdResponse.Value);
            
            Assert.Equal(1, createdTicket.UserId);
            Assert.Equal(request.SeatNumber, createdTicket.SeatNumber);
            Assert.Equal(49, mockShowTime.AvailableSeats); // Checks seat decrement logic
            
            _mockTicketRepo.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Once);
            _mockShowTimeRepo.Verify(r => r.UpdateAsync(mockShowTime), Times.Once);
        }

        [Fact]
        public async Task CancelTicket_WhenValidTicket_ReturnsOk_AndIncrementsSeats()
        {
            // Arrange
            int ticketId = 1;
            var mockTicket = new Ticket { Id = ticketId, UserId = 1, ShowTimeId = 1, Status = TicketStatus.Active };
            var mockShowTime = new ShowTime { Id = 1, AvailableSeats = 49 };

            _mockTicketRepo.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(mockTicket);
            _mockShowTimeRepo.Setup(repo => repo.GetByIdAsync(mockTicket.ShowTimeId)).ReturnsAsync(mockShowTime);

            // Act
            var result = await _controller.CancelTicket(ticketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(TicketStatus.Cancelled, mockTicket.Status);
            Assert.Equal(50, mockShowTime.AvailableSeats); // Checks seat increment logic

            _mockTicketRepo.Verify(r => r.UpdateAsync(mockTicket), Times.Once);
            _mockShowTimeRepo.Verify(r => r.UpdateAsync(mockShowTime), Times.Once);
        }
    }
}
