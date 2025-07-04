using Xunit;
using PredictionService.Controllers;
using PredictionService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace PredictionService.Tests
{
    public class PredictionControllerTests
    {
        [Fact]
        public async Task GetPredictions_ReturnsAllPredictions()
        {
            // Arrange
            var predictions = new List<Prediction> { new Prediction { Id = 1, UserId = 1, ParkingSpot = "A1" } };
            var dbSetMock = DbContextMockHelper.GetQueryableMockDbSet(predictions);
            var contextMock = new Mock<PredictionDbContext>(new DbContextOptions<PredictionDbContext>());
            contextMock.Setup(c => c.Predictions).Returns(dbSetMock);
            var controller = new PredictionController(contextMock.Object);

            // Act
            var result = await controller.GetPredictions();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Prediction>>>(result);
            var returnValue = Assert.IsType<List<Prediction>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal("A1", returnValue[0].ParkingSpot);
        }

        [Fact]
        public async Task PostPrediction_AddsPrediction()
        {
            // Arrange
            var predictions = new List<Prediction>();
            var dbSetMock = DbContextMockHelper.GetQueryableMockDbSet(predictions);
            var contextMock = new Mock<PredictionDbContext>(new DbContextOptions<PredictionDbContext>());
            contextMock.Setup(c => c.Predictions).Returns(dbSetMock);
            contextMock.Setup(c => c.Add(It.IsAny<Prediction>())).Callback<Prediction>(p => predictions.Add(p));
            contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            var controller = new PredictionController(contextMock.Object);
            var newPrediction = new Prediction { UserId = 2, ParkingSpot = "B2" };

            // Act
            var result = await controller.PostPrediction(newPrediction);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<Prediction>(createdAt.Value);
            Assert.Equal("B2", returnValue.ParkingSpot);
            Assert.Single(predictions);
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