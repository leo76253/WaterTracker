using System.Net.Http.Json;

namespace WaterTracker.Services;

public class WaterIntakeService(HttpClient http)
{
    public async Task<List<WaterIntakeResponse>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<WaterIntakeResponse>>("api/waterintake")
               ?? [];
    }

    public async Task<WaterIntakeResponse?> GetByIdAsync(Guid id)
    {
        try
        {
            return await http.GetFromJsonAsync<WaterIntakeResponse>($"api/waterintake/{id:D}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<WaterIntakeResponse?> CreateAsync(CreateWaterIntakeRequest request)
    {
        var response = await http.PostAsJsonAsync("api/waterintake", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<WaterIntakeResponse>();
    }

    public async Task<WaterIntakeResponse?> UpdateAsync(Guid id, UpdateWaterIntakeRequest request)
    {
        var response = await http.PutAsJsonAsync($"api/waterintake/{id:D}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<WaterIntakeResponse>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"api/waterintake/{id:D}");
        return response.IsSuccessStatusCode;
    }
}
