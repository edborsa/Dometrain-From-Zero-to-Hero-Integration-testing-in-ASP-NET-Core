using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customers.Api.Tests.Integration;

public class CustomerControllerTests
{
    private readonly WebApplicationFactory<IAPIMarker> _appFactory = new();
    private readonly HttpClient _httpClient;

    public CustomerControllerTests()
    {
        _httpClient = _appFactory.CreateClient();
    }

    [Fact]
    public async Task Customer_Returns_NotFound_When_Customer_Does_NOT_Exists()
    {
        var response = await _httpClient.GetAsync($"customers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}