using System.Net;

namespace Uvidik.Api.IntegrationTests;

[Collection(nameof(ApiTestCollection))]
public class ExceptionHandlingTests(UvidikApiFactory factory)
{
    [Theory]
    [InlineData("/_test/throws/validation", HttpStatusCode.BadRequest)]
    [InlineData("/_test/throws/not-found", HttpStatusCode.NotFound)]
    [InlineData("/_test/throws/conflict", HttpStatusCode.Conflict)]
    [InlineData("/_test/throws/forbidden", HttpStatusCode.Forbidden)]
    [InlineData("/_test/throws/unexpected", HttpStatusCode.InternalServerError)]
    public async Task ThrownException_MapsToExpectedProblemDetailsResponse(string path, HttpStatusCode expectedStatus)
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("   at ", body);
        Assert.DoesNotContain("Uvidik.Api.Testing", body);
    }

    [Fact]
    public async Task ValidationFailure_IncludesFieldLevelErrors()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/_test/throws/validation");

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"field\"", body);
    }
}
