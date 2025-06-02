namespace TestingDemo.Tests;

public static class HttpExtensions
{
    public static async Task<T?> ReadAsJsonAsync<T>(this HttpContent content)
    {
        var json = await content.ReadAsStringAsync();
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
    }
}
