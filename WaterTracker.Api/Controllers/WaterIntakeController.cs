using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using WaterTracker.Api.Dtos;
using WaterTracker.Api.Services.Interfaces;
using static WaterTracker.Api.Dtos.WaterIntakeDtos;

namespace WaterTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WaterIntakeController(IWaterIntakeService waterIntakeService) : ControllerBase
    {
        private string? GetUserId() =>
            User.FindFirstValue(OpenIddictConstants.Claims.Subject)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await waterIntakeService.GeAllWaterIntakesForUserAsync(GetUserId());
            if (!result.Success)
                return BadRequest(new { error = result.Error });
            return Ok(result.Data);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await waterIntakeService.GetWaterIntakeByIdAsync(id, GetUserId());
            if (!result.Success)
                return BadRequest(new { error = result.Error });
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWaterIntakeRequest waterIntake)
        {
            var userId = GetUserId();
            var result = await waterIntakeService.CreateWaterIntakeAsync(waterIntake, userId);
            if (!result.Success)
                return BadRequest(new { error = result.Error });
            return CreatedAtAction(nameof(Get), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWaterIntakeRequest waterIntake)
        {
            var userId = GetUserId();

            var result = await waterIntakeService.UpdateWaterIntakeAsync(id, waterIntake, userId);
            if (!result.Success)
                return BadRequest(new { error = result.Error });
            return Ok(result.Data);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();

            var result = await waterIntakeService.DeleteWaterIntakeAsync(id, userId);
            if (!result.Success)
                return BadRequest(new { error = result.Error });
            return NoContent();
        }

    }
}