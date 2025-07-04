using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationService.Models;
using ReservationService.RabbitMQ;
using System.Net.Http;
using System.Net;

namespace ReservationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly ReservationDbContext _context;
        private readonly IRabbitMqPublisher _publisher;
        private readonly IHttpClientFactory _httpClientFactory;
        public ReservationController(ReservationDbContext context, IRabbitMqPublisher publisher, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _publisher = publisher;
            _httpClientFactory = httpClientFactory;
        }

        // GET: api/reservation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations.ToListAsync();
        }

        // GET: api/reservation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();
            return reservation;
        }

        // POST: api/reservation
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(Reservation reservation)
        {
            // Walidacja użytkownika przez HTTP
            var client = _httpClientFactory.CreateClient();
            var userResponse = await client.GetAsync($"http://localhost:5000/api/user/{reservation.UserId}");
            if (userResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest($"User with id {reservation.UserId} does not exist.");
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Publikuj zdarzenie do RabbitMQ
            var eventMessage = System.Text.Json.JsonSerializer.Serialize(new {
                reservation.Id,
                reservation.UserId,
                reservation.ParkingSpot,
                reservation.StartTime,
                reservation.EndTime,
                reservation.Status
            });
            _publisher.Publish(eventMessage);

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/reservation/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, Reservation reservation)
        {
            if (id != reservation.Id)
                return BadRequest();
            _context.Entry(reservation).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reservations.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/reservation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 