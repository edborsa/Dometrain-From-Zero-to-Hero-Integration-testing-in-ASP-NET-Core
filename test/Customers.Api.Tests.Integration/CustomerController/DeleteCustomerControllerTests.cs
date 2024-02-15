using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;

namespace Customers.Api.Tests.Integration.CustomerController;

public class DeleteCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public DeleteCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task Delete_ReturnsOK_WhenCustomerExists()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var response = await _client.PostAsJsonAsync("customers", customer);
        var createCustomerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        // Act
        var deleteResponse = await _client.DeleteAsync($"customers/{createCustomerResponse!.Id}");
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenCustomerDoesNotExists()
    {
        // Act
        var getResponse = await _client.DeleteAsync($"customers/{Guid.NewGuid()}");
        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}