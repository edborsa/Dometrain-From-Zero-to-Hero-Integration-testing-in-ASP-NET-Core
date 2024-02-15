using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;

namespace Customers.Api.Tests.Integration.CustomerController;

public class GetCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public GetCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsACurstomer_WhenCustomerExists()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var response = await _client.PostAsJsonAsync("customers", customer);
        var createCustomerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        // Act
        var getResponse = await _client.GetAsync($"customers/{createCustomerResponse!.Id}");
        // Assert
        var getCustomerResponse = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        getCustomerResponse.Should().BeEquivalentTo(customer);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExists()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        // Act
        var getResponse = await _client.GetAsync($"customers/{Guid.NewGuid()}");
        // Assert
        var getCustomerResponse = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
}