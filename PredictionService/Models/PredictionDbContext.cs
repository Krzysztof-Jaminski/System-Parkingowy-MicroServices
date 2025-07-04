using Microsoft.EntityFrameworkCore;

namespace PredictionService.Models
{
    public class PredictionDbContext : DbContext
    {
        public PredictionDbContext(DbContextOptions<PredictionDbContext> options) : base(options) { }

        public DbSet<Prediction> Predictions { get; set; }
    }
} 