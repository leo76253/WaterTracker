using Microsoft.EntityFrameworkCore;
using WaterTracker.Api.Common;
using WaterTracker.Api.Data;
using WaterTracker.Api.Models;
using WaterTracker.Api.Services;
using static WaterTracker.Api.Dtos.WaterIntakeDtos;

namespace WaterTracker.Tests;

public class WaterIntakeServiceTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoIntakes()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var service = new WaterIntakeService(context);

        var result = await service.GeAllWaterIntakesForUserAsync("user-1");

        Assert.True(result.Success);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyUserIntakes()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        context.WaterIntakes.AddRange(
            new WaterIntake { Id = Guid.NewGuid(), CustomerId = "user-1", Amount = 1.0, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new WaterIntake { Id = Guid.NewGuid(), CustomerId = "user-2", Amount = 2.0, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var service = new WaterIntakeService(context);

        var result = await service.GeAllWaterIntakesForUserAsync("user-1");

        Assert.True(result.Success);
        Assert.Single(result.Data!);
        Assert.Equal(1.0, result.Data![0].Amount);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedIntakes()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        context.WaterIntakes.AddRange(
            new WaterIntake { Id = Guid.NewGuid(), CustomerId = "user-1", Amount = 1.0, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new WaterIntake { Id = Guid.NewGuid(), CustomerId = "user-1", Amount = 2.0, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, DeletedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var service = new WaterIntakeService(context);

        var result = await service.GeAllWaterIntakesForUserAsync("user-1");

        Assert.Single(result.Data!);
        Assert.Equal(1.0, result.Data![0].Amount);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsIntake_WhenFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var id = Guid.NewGuid();
        context.WaterIntakes.Add(new WaterIntake { Id = id, CustomerId = "user-1", Amount = 1.5, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        context.SaveChanges();
        var service = new WaterIntakeService(context);

        var result = await service.GetWaterIntakeByIdAsync(id, "user-1");

        Assert.True(result.Success);
        Assert.Equal(1.5, result.Data!.Amount);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFailure_WhenNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var service = new WaterIntakeService(context);

        var result = await service.GetWaterIntakeByIdAsync(Guid.NewGuid(), "user-1");

        Assert.False(result.Success);
        Assert.Equal("Water intake not found", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFailure_WhenDeleted()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var id = Guid.NewGuid();
        context.WaterIntakes.Add(new WaterIntake { Id = id, CustomerId = "user-1", Amount = 1.0, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, DeletedAt = DateTime.UtcNow });
        context.SaveChanges();
        var service = new WaterIntakeService(context);

        var result = await service.GetWaterIntakeByIdAsync(id, "user-1");

        Assert.False(result.Success);
        Assert.Equal("Water intake not found", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFailure_WhenWrongUser()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var id = Guid.NewGuid();
        context.WaterIntakes.Add(new WaterIntake { Id = id, CustomerId = "user-1", Amount = 1.0, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        context.SaveChanges();
        var service = new WaterIntakeService(context);

        var result = await service.GetWaterIntakeByIdAsync(id, "user-2");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_CreatesIntake()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var service = new WaterIntakeService(context);

        var result = await service.CreateWaterIntakeAsync(new CreateWaterIntakeRequest(2.5, DateTime.UtcNow), "user-1");

        Assert.True(result.Success);
        Assert.Equal(2.5, result.Data!.Amount);
        Assert.Equal(1, context.WaterIntakes.Count());
    }

    [Fact]
    public async Task CreateAsync_ReturnsFailure_ForNegativeAmount()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var service = new WaterIntakeService(context);

        var result = await service.CreateWaterIntakeAsync(new CreateWaterIntakeRequest(-1.0, DateTime.UtcNow), "user-1");

        Assert.False(result.Success);
        Assert.Equal("Invalid water intake amount", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesIntake()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var id = Guid.NewGuid();
        context.WaterIntakes.Add(new WaterIntake { Id = id, CustomerId = "user-1", Amount = 1.0, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        context.SaveChanges();
        var service = new WaterIntakeService(context);

        var result = await service.UpdateWaterIntakeAsync(id, new UpdateWaterIntakeRequest(5.0, DateTime.UtcNow), "user-1");

        Assert.True(result.Success);
        Assert.Equal(5.0, result.Data!.Amount);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var service = new WaterIntakeService(context);

        var result = await service.UpdateWaterIntakeAsync(Guid.NewGuid(), new UpdateWaterIntakeRequest(1.0, DateTime.UtcNow), "user-1");

        Assert.False(result.Success);
        Assert.Equal("Water intake not found", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_ForNegativeAmount()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var id = Guid.NewGuid();
        context.WaterIntakes.Add(new WaterIntake { Id = id, CustomerId = "user-1", Amount = 1.0, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        context.SaveChanges();
        var service = new WaterIntakeService(context);

        var result = await service.UpdateWaterIntakeAsync(id, new UpdateWaterIntakeRequest(-1.0, DateTime.UtcNow), "user-1");

        Assert.False(result.Success);
        Assert.Equal("Invalid water intake amount", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesIntake()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var id = Guid.NewGuid();
        context.WaterIntakes.Add(new WaterIntake { Id = id, CustomerId = "user-1", Amount = 1.0, Date = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        context.SaveChanges();
        var service = new WaterIntakeService(context);

        var result = await service.DeleteWaterIntakeAsync(id, "user-1");

        Assert.True(result.Success);
        Assert.NotNull(context.WaterIntakes.Find(id)!.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = CreateDbContext(dbName);
        var service = new WaterIntakeService(context);

        var result = await service.DeleteWaterIntakeAsync(Guid.NewGuid(), "user-1");

        Assert.False(result.Success);
        Assert.Equal("Water intake not found", result.Error);
    }
}
