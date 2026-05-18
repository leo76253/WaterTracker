namespace WaterTracker.Api.Models
{
    public class WaterIntake
    {
        public Guid Id { get; set; }

        // NOTE: Package (OpenIddict) uses sring to store a guid, this could be changed to a guid but keeping it as string as its less work and not really needed given the time constraints/nature of the application.
        public string CustomerId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double Amount { get; set; }

        // While there is already a date field, we can still have these for tracking purposes, ie if a admin needs to update the record.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}