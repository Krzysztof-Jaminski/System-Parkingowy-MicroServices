using Xunit;
using UserService.Controllers;
using UserService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace UserService.Tests
{
    public class UserControllerTests
    {
        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User> { new User { Id = 1, Name = "Test", Email = "test@example.com" } };
            var dbSetMock = DbContextMockHelper.GetQueryableMockDbSet(users);
            var contextMock = new Mock<UserDbContext>(new DbContextOptions<UserDbContext>());
            contextMock.Setup(c => c.Users).Returns(dbSetMock);
            var controller = new UserController(contextMock.Object);

            // Act
            var result = await controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var returnValue = Assert.IsType<List<User>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal("test@example.com", returnValue[0].Email);
        }

        [Fact]
        public async Task PostUser_AddsUser()
        {
            // Arrange
            var users = new List<User>();
            var dbSetMock = DbContextMockHelper.GetQueryableMockDbSet(users);
            var contextMock = new Mock<UserDbContext>(new DbContextOptions<UserDbContext>());
            contextMock.Setup(c => c.Users).Returns(dbSetMock);
            contextMock.Setup(c => c.Add(It.IsAny<User>())).Callback<User>(u => users.Add(u));
            contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            var controller = new UserController(contextMock.Object);
            var newUser = new User { Name = "Nowy", Email = "nowy@example.com" };

            // Act
            var result = await controller.PostUser(newUser);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<User>(createdAt.Value);
            Assert.Equal("nowy@example.com", returnValue.Email);
            Assert.Single(users);
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