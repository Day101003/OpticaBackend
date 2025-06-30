namespace ProyectoFinal.Models
{
    public class QuoteDto
    {
        public required string ClientName { get; set; }
        public required string ClientEmail { get; set; }
        public required string ClientPhone { get; set; }
        public int AvailabilityId { get; set; }
        public string? Notes { get; set; }
    }
}