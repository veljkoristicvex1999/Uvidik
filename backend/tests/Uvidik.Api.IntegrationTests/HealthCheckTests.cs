namespace Uvidik.Api.IntegrationTests;

[Collection(nameof(ApiTestCollection))]
public class HealthCheckTests(UvidikApiFactory factory)
{
    [Fact]
    public async Task Health_ReturnsHealthy_WithoutRequiringDatabase()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task HealthReady_ReturnsHealthy_WhenDatabaseReachable()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
