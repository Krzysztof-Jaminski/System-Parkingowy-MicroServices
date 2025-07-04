namespace PredictionService.Models
{
    public class Prediction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double PredictedOccupancy { get; set; } // procent zajętości
        public int ReservationCount { get; set; } // liczba rezerwacji na dany dzień
        public int UserId { get; set; }
        public string ParkingSpot { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 