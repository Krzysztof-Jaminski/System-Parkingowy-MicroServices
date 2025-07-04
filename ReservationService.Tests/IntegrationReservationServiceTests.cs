using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using ReservationService.Models;
using Xunit;

namespace ReservationService.Tests
{
    public class IntegrationReservationServiceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public IntegrationReservationServiceTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CanCreateAndGetReservation()
        {
            var reservation = new Reservation { UserId = 1, ParkingSpot = "B2", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var postResponse = await _client.PostAsJsonAsync("/api/reservation", reservation);
            postResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/reservation");
            getResponse.EnsureSuccessStatusCode();
            var reservations = await getResponse.Content.ReadFromJsonAsync<Reservation[]>();
            Assert.Contains(reservations, r => r.ParkingSpot == "B2");
        }

        [Fact]
        public async Task CanGetReservationById()
        {
            var reservation = new Reservation { UserId = 2, ParkingSpot = "C2", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var postResponse = await _client.PostAsJsonAsync("/api/reservation", reservation);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<Reservation>();

            var getResponse = await _client.GetAsync($"/api/reservation/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var fetched = await getResponse.Content.ReadFromJsonAsync<Reservation>();
            Assert.Equal("C2", fetched.ParkingSpot);
        }

        [Fact]
        public async Task CanUpdateReservation()
        {
            var reservation = new Reservation { UserId = 3, ParkingSpot = "D2", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var postResponse = await _client.PostAsJsonAsync("/api/reservation", reservation);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<Reservation>();

            created.Status = "Updated";
            var putResponse = await _client.PutAsJsonAsync($"/api/reservation/{created.Id}", created);
            putResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/reservation/{created.Id}");
            var updated = await getResponse.Content.ReadFromJsonAsync<Reservation>();
            Assert.Equal("Updated", updated.Status);
        }

        [Fact]
        public async Task CanDeleteReservation()
        {
            var reservation = new Reservation { UserId = 4, ParkingSpot = "E2", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var postResponse = await _client.PostAsJsonAsync("/api/reservation", reservation);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<Reservation>();

            var deleteResponse = await _client.DeleteAsync($"/api/reservation/{created.Id}");
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/reservation/{created.Id}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetReservation_NotFound_Returns404()
        {
            var getResponse = await _client.GetAsync("/api/reservation/99999");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task PutReservation_BadRequest_Returns400()
        {
            var reservation = new Reservation { Id = 999, UserId = 5, ParkingSpot = "F2", StartTime = System.DateTime.UtcNow, EndTime = System.DateTime.UtcNow.AddHours(1), Status = "Active" };
            var putResponse = await _client.PutAsJsonAsync("/api/reservation/1", reservation); // id != reservation.Id
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, putResponse.StatusCode);
        }
    }
} 