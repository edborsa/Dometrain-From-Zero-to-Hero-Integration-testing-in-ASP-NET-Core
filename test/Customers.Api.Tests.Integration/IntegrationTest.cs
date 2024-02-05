using System.Net;

namespace Customers.Api.Tests.Integration;

public class IntegrationTest
{
    [Fact]
    public async Task Test()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:5001")
        };

        var response = await httpClient.GetAsync($"customers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}