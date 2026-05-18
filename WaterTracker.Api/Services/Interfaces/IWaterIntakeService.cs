using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterTracker.Api.Common;
using static WaterTracker.Api.Dtos.WaterIntakeDtos;

namespace WaterTracker.Api.Services.Interfaces
{
    public interface IWaterIntakeService
    {
        Task<Result<List<WaterIntakeResponse>>> GeAllWaterIntakesForUserAsync(string customerId);
        Task<Result<WaterIntakeResponse>> GetWaterIntakeByIdAsync(Guid id, string customerId);
        Task<Result<WaterIntakeResponse>> CreateWaterIntakeAsync(CreateWaterIntakeRequest request, string customerId);
        Task<Result<WaterIntakeResponse>> UpdateWaterIntakeAsync(Guid id, UpdateWaterIntakeRequest request, string customerId);
        Task<Result<bool>> DeleteWaterIntakeAsync(Guid id, string customerId);
    }
}