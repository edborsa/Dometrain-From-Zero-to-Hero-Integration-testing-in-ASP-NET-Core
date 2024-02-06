using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using Customers.Api.Domain.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customers.Api.Tests.Integration;

public class CustomerControllerTests : IClassFixture<WebApplicationFactory<IAPIMarker>>
{
    private readonly HttpClient _httpClient;
    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.GitHubUsername, faker => "nickchapsas")
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public CustomerControllerTests(WebApplicationFactory<IAPIMarker> appFactory)
    {
        _httpClient = appFactory.CreateClient();
    }

    [Fact]
    public async Task Customer_Returns_NotFound_When_Customer_Does_NOT_Exists()
    {
        var response = await _httpClient.GetAsync($"customers/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Title.Should().Be("Not Found");
        problem!.Status.Should().Be(404);

    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenCustomerIsCreated()
    {
        var customer = _customerGenerator.Generate();
        
        var response = await _httpClient.PostAsJsonAsync("customers", customer);

        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(customer);
        response.StatusCode.Should().Be(HttpStatusCode.Created);



    }
}