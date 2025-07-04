using Xunit;
using ReservationService.Controllers;
using ReservationService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace ReservationService.Tests
{
    public class ReservationControllerTests
    {
        // Przykładowy test jednostkowy (do rozbudowy)
        [Fact]
        public void DummyUnitTest()
        {
            Assert.True(true);
        }

        [Fact]
        public async Task GetReservations_ReturnsAllReservations()
        {
            // Arrange
            var reservations = new List<Reservation> { new Reservation { Id = 1, UserId = 1, ParkingSpot = "A1" } };
            var dbSetMock = DbContextMockHelper.GetQueryableMockDbSet(reservations);
            var contextMock = new Mock<ReservationDbContext>(new DbContextOptions<ReservationDbContext>());
            contextMock.Setup(c => c.Reservations).Returns(dbSetMock);
            var publisherMock = new Mock<ReservationService.RabbitMQ.IRabbitMqPublisher>();
            var httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
            var controller = new ReservationController(contextMock.Object, publisherMock.Object, httpClientFactoryMock.Object);

            // Act
            var result = await controller.GetReservations();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Reservation>>>(result);
            var returnValue = Assert.IsType<List<Reservation>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal("A1", returnValue[0].ParkingSpot);
        }

        [Fact]
        public async Task PostReservation_AddsReservation()
        {
            // Arrange
            var reservations = new List<Reservation>();
            var dbSetMock = DbContextMockHelper.GetQueryableMockDbSet(reservations);
            var contextMock = new Mock<ReservationDbContext>(new DbContextOptions<ReservationDbContext>());
            contextMock.Setup(c => c.Reservations).Returns(dbSetMock);
            contextMock.Setup(c => c.Add(It.IsAny<Reservation>())).Callback<Reservation>(r => reservations.Add(r));
            contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            var publisherMock = new Mock<ReservationService.RabbitMQ.IRabbitMqPublisher>();
            var httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
            var controller = new ReservationController(contextMock.Object, publisherMock.Object, httpClientFactoryMock.Object);
            var newReservation = new Reservation { UserId = 2, ParkingSpot = "B2" };

            // Act
            var result = await controller.PostReservation(newReservation);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<Reservation>(createdAt.Value);
            Assert.Equal("B2", returnValue.ParkingSpot);
            Assert.Single(reservations);
        }
    }

    // Pomocnicza klasa do mockowania DbSet
    public static class DbContextMockHelper
    {
        public static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return dbSet.Object;
        }
    }
} 