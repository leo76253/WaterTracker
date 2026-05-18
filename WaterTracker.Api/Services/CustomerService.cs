using Microsoft.AspNetCore.Identity;
using WaterTracker.Api.Common;
using WaterTracker.Api.Dtos;
using WaterTracker.Api.Models;
using WaterTracker.Api.Services.Interfaces;

namespace WaterTracker.Api.Services
{
    public class CustomerService(UserManager<Customer> userManager) : ICustomerService
    {
        public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Result<RegisterResponse>.Failure("A user with this email already exists.");

            var customer = new Customer
            {
                UserName = request.Email,
                Email = request.Email,
                Forename = request.Forename,
                Surname = request.Surname,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(customer, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<RegisterResponse>.Failure(errors);
            }

            return Result<RegisterResponse>.SuccessResult(
                new RegisterResponse(customer.Id, customer.Email!, customer.Forename, customer.Surname));
        }
    }
}
