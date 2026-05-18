using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterTracker.Api.Common;
using WaterTracker.Api.Data;
using WaterTracker.Api.Dtos;
using WaterTracker.Api.Models;
using WaterTracker.Api.Services.Interfaces;
using static WaterTracker.Api.Dtos.WaterIntakeDtos;

namespace WaterTracker.Api.Services
{
    public class WaterIntakeService(AppDbContext context) : IWaterIntakeService
    {
        public Task<Result<List<WaterIntakeResponse>>> GeAllWaterIntakesForUserAsync(string customerId)
        {
            var result = context.WaterIntakes
                .Where(w => w.CustomerId == customerId && w.DeletedAt == null)
                .Select(w => MapToResponse(w))
                .ToList();
            return Task.FromResult(Result<List<WaterIntakeResponse>>.SuccessResult(result));
        }

        public Task<Result<WaterIntakeResponse>> GetWaterIntakeByIdAsync(Guid id, string customerId)
        {
            var waterIntake = context.WaterIntakes.FirstOrDefault(w => w.Id == id && w.CustomerId == customerId && w.DeletedAt == null);
            if (waterIntake == null)
                return Task.FromResult(Result<WaterIntakeResponse>.Failure("Water intake not found"));

            var response = MapToResponse(waterIntake);
            return Task.FromResult(Result<WaterIntakeResponse>.SuccessResult(response));
        }

        public Task<Result<WaterIntakeResponse>> CreateWaterIntakeAsync(CreateWaterIntakeRequest request, string customerId)
        {
            if (!IsValidAmount(request.Amount))
                return Task.FromResult(Result<WaterIntakeResponse>.Failure("Invalid water intake amount"));

            var waterIntake = new WaterIntake
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                Date = request.Date,
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.WaterIntakes.Add(waterIntake);
            var saved = context.SaveChanges();
            if (saved == 0)
                return Task.FromResult(Result<WaterIntakeResponse>.Failure("Failed to create water intake"));

            var response = MapToResponse(waterIntake);
            return Task.FromResult(Result<WaterIntakeResponse>.SuccessResult(response));
        }

        public Task<Result<WaterIntakeResponse>> UpdateWaterIntakeAsync(Guid id, UpdateWaterIntakeRequest request, string customerId)
        {
            var waterIntake = context.WaterIntakes.FirstOrDefault(w => w.Id == id && w.CustomerId == customerId && w.DeletedAt == null);
            if (waterIntake == null)
                return Task.FromResult(Result<WaterIntakeResponse>.Failure("Water intake not found"));

            if (!IsValidAmount(request.Amount))
                return Task.FromResult(Result<WaterIntakeResponse>.Failure("Invalid water intake amount"));

            waterIntake.Amount = request.Amount;
            waterIntake.Date = request.Date;
            waterIntake.UpdatedAt = DateTime.UtcNow;

            var saved = context.SaveChanges();
            if (saved == 0)
                return Task.FromResult(Result<WaterIntakeResponse>.Failure("Failed to update water intake"));

            var response = MapToResponse(waterIntake);
            return Task.FromResult(Result<WaterIntakeResponse>.SuccessResult(response));
        }

        public Task<Result<bool>> DeleteWaterIntakeAsync(Guid id, string customerId)
        {
            var waterIntake = context.WaterIntakes.FirstOrDefault(w => w.Id == id && w.CustomerId == customerId && w.DeletedAt == null);
            if (waterIntake == null)
                return Task.FromResult(Result<bool>.Failure("Water intake not found"));

            waterIntake.DeletedAt = DateTime.UtcNow;
            var saved = context.SaveChanges();
            if (saved == 0)
                return Task.FromResult(Result<bool>.Failure("Failed to delete water intake"));

            return Task.FromResult(Result<bool>.SuccessBool());
        }

        private static WaterIntakeResponse MapToResponse(WaterIntake waterIntake)
        {
            return new WaterIntakeResponse(
                waterIntake.Id,
                waterIntake.Amount,
                waterIntake.Date,
                waterIntake.UpdatedAt
            );
        }

        // Unsure if there are more rules so putting here for extendability.
        private static bool IsValidAmount(double amount) => amount >= 0;
    }
}