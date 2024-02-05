using System.Net;

namespace Customers.Api.Tests.Integration;

public class CustomerControllerTests
{
    [Fact]
    public async Task Customer_Returns_NotFound_When_Customer_Does_NOT_Exists()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:5001")
        };

        var response = await httpClient.GetAsync($"customers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}