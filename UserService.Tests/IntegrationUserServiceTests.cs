using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using UserService.Models;
using Xunit;

namespace UserService.Tests
{
    public class IntegrationUserServiceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public IntegrationUserServiceTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CanCreateAndGetUser()
        {
            var user = new User { Name = "Test User", Email = "test@example.com" };
            var postResponse = await _client.PostAsJsonAsync("/api/user", user);
            postResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/user");
            getResponse.EnsureSuccessStatusCode();
            var users = await getResponse.Content.ReadFromJsonAsync<User[]>();
            Assert.Contains(users, u => u.Email == "test@example.com");
        }

        [Fact]
        public async Task CanGetUserById()
        {
            var user = new User { Name = "Test2", Email = "test2@example.com" };
            var postResponse = await _client.PostAsJsonAsync("/api/user", user);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<User>();

            var getResponse = await _client.GetAsync($"/api/user/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var fetched = await getResponse.Content.ReadFromJsonAsync<User>();
            Assert.Equal("test2@example.com", fetched.Email);
        }

        [Fact]
        public async Task CanUpdateUser()
        {
            var user = new User { Name = "ToUpdate", Email = "update@example.com" };
            var postResponse = await _client.PostAsJsonAsync("/api/user", user);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<User>();

            created.Name = "UpdatedName";
            var putResponse = await _client.PutAsJsonAsync($"/api/user/{created.Id}", created);
            putResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/user/{created.Id}");
            var updated = await getResponse.Content.ReadFromJsonAsync<User>();
            Assert.Equal("UpdatedName", updated.Name);
        }

        [Fact]
        public async Task CanDeleteUser()
        {
            var user = new User { Name = "ToDelete", Email = "delete@example.com" };
            var postResponse = await _client.PostAsJsonAsync("/api/user", user);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<User>();

            var deleteResponse = await _client.DeleteAsync($"/api/user/{created.Id}");
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/user/{created.Id}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetUser_NotFound_Returns404()
        {
            var getResponse = await _client.GetAsync("/api/user/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task PutUser_BadRequest_Returns400()
        {
            var user = new User { Id = 999, Name = "Bad", Email = "bad@example.com" };
            var putResponse = await _client.PutAsJsonAsync("/api/user/1", user);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, putResponse.StatusCode);
        }
    }
} 