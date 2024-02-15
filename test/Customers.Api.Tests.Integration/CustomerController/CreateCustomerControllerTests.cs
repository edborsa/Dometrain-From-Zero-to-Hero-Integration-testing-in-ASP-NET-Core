using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Customers.Api.Tests.Integration.CustomerController;

public class CreateCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly CustomerApiFactory _apiFactory;
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public CreateCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task Create_CreatesUser_WhenDataIsValid()
    {
        var customer = _customerGenerator.Generate();
        var response = await _client.PostAsJsonAsync("customers", customer);
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(customer);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should()
            .Be($"http://localhost/customers/{customerResponse!.Id}");
    }
}