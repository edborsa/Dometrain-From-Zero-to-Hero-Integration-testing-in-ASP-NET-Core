using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;

namespace Customers.Api.Tests.Integration.CustomerController;

public class GetAllCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public GetAllCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllCurstomers_WhenCustomersExists()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var response = await _client.PostAsJsonAsync("customers", customer);
        var createCustomerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        // Act
        var getResponse = await _client.GetAsync("customers");
        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getCustomerResponse = await getResponse.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
        getCustomerResponse!.Customers.Single().Should().BeEquivalentTo(customer);
        
        // Cleanup
        await _client.DeleteAsync($"customers/{createCustomerResponse!.Id}");
    }
    
    [Fact]
    public async Task GetAll_ReturnsEmpty_WhenNoCustomersExists()
    {
        // Act
        var getResponse = await _client.GetAsync("customers");
        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getCustomerResponse = await getResponse.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
        getCustomerResponse!.Customers.Should().BeEmpty();
    }
    
}