namespace WaterTracker.Services;

public record WaterIntakeResponse(Guid Id, double Amount, DateTime Date, DateTime UpdatedAt);
public record CreateWaterIntakeRequest(double Amount, DateTime Date);
public record UpdateWaterIntakeRequest(double Amount, DateTime Date);
