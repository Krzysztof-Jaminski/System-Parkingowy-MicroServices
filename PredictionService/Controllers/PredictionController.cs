using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PredictionService.Models;

namespace PredictionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictionController : ControllerBase
    {
        private readonly PredictionDbContext _context;
        public PredictionController(PredictionDbContext context)
        {
            _context = context;
        }

        // GET: api/prediction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prediction>>> GetPredictions()
        {
            return await _context.Predictions.ToListAsync();
        }

        // GET: api/prediction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Prediction>> GetPrediction(int id)
        {
            var prediction = await _context.Predictions.FindAsync(id);
            if (prediction == null)
                return NotFound();
            return prediction;
        }

        // POST: api/prediction
        [HttpPost]
        public async Task<ActionResult<Prediction>> PostPrediction(Prediction prediction)
        {
            prediction.CreatedAt = DateTime.UtcNow;
            _context.Predictions.Add(prediction);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPrediction), new { id = prediction.Id }, prediction);
        }

        // PUT: api/prediction/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrediction(int id, Prediction prediction)
        {
            if (id != prediction.Id)
                return BadRequest();
            _context.Entry(prediction).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Predictions.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/prediction/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrediction(int id)
        {
            var prediction = await _context.Predictions.FindAsync(id);
            if (prediction == null)
                return NotFound();
            _context.Predictions.Remove(prediction);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 