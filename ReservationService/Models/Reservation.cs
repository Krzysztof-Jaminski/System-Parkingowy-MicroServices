namespace ReservationService.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ParkingSpot { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } // np. Confirmed, Cancelled
    }
} 