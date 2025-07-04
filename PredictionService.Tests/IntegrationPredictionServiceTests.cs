using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using PredictionService.Models;
using Xunit;

namespace PredictionService.Tests
{
    public class IntegrationPredictionServiceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public IntegrationPredictionServiceTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CanCreateAndGetPrediction()
        {
            var prediction = new Prediction { UserId = 1, ParkingSpot = "A1", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var postResponse = await _client.PostAsJsonAsync("/api/prediction", prediction);
            postResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/prediction");
            getResponse.EnsureSuccessStatusCode();
            var predictions = await getResponse.Content.ReadFromJsonAsync<Prediction[]>();
            Assert.Contains(predictions, p => p.ParkingSpot == "A1");
        }

        [Fact]
        public async Task CanGetPredictionById()
        {
            var prediction = new Prediction { UserId = 2, ParkingSpot = "C1", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var postResponse = await _client.PostAsJsonAsync("/api/prediction", prediction);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<Prediction>();

            var getResponse = await _client.GetAsync($"/api/prediction/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var fetched = await getResponse.Content.ReadFromJsonAsync<Prediction>();
            Assert.Equal("C1", fetched.ParkingSpot);
        }

        [Fact]
        public async Task CanUpdatePrediction()
        {
            var prediction = new Prediction { UserId = 3, ParkingSpot = "D1", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var postResponse = await _client.PostAsJsonAsync("/api/prediction", prediction);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<Prediction>();

            created.Status = "Updated";
            var putResponse = await _client.PutAsJsonAsync($"/api/prediction/{created.Id}", created);
            putResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/prediction/{created.Id}");
            var updated = await getResponse.Content.ReadFromJsonAsync<Prediction>();
            Assert.Equal("Updated", updated.Status);
        }

        [Fact]
        public async Task CanDeletePrediction()
        {
            var prediction = new Prediction { UserId = 4, ParkingSpot = "E1", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var postResponse = await _client.PostAsJsonAsync("/api/prediction", prediction);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<Prediction>();

            var deleteResponse = await _client.DeleteAsync($"/api/prediction/{created.Id}");
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/prediction/{created.Id}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetPrediction_NotFound_Returns404()
        {
            var getResponse = await _client.GetAsync("/api/prediction/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task PutPrediction_BadRequest_Returns400()
        {
            var prediction = new Prediction { Id = 999, UserId = 5, ParkingSpot = "F1", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var putResponse = await _client.PutAsJsonAsync("/api/prediction/1", prediction); // id != prediction.Id
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, putResponse.StatusCode);
        }
    }
} 