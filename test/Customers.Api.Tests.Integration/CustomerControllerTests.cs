using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customers.Api.Tests.Integration;

public class CustomerControllerTests : IClassFixture<WebApplicationFactory<IAPIMarker>>
{
    private readonly HttpClient _httpClient;

    public CustomerControllerTests(WebApplicationFactory<IAPIMarker> appFactory)
    {
        _httpClient = appFactory.CreateClient();
    }

    [Fact]
    public async Task Customer_Returns_NotFound_When_Customer_Does_NOT_Exists()
    {
        var response = await _httpClient.GetAsync($"customers/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("404");
    }
}