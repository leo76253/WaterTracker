using WaterTracker.Api.Common;
using WaterTracker.Api.Dtos;

namespace WaterTracker.Api.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);
    }
}
