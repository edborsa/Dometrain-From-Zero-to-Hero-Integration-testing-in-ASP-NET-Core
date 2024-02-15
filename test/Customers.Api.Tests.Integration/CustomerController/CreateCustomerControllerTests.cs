using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Customers.Api.Tests.Integration.CustomerController;

public class CreateCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public CreateCustomerControllerTests(CustomerApiFactory apiFactory)
    {
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
    
    [Fact]
    public async Task Create_ReturnsValidationError_WhenEmailIsInvalid()
    {
        const string invalidEmail = "asdfasfs";
        var customer = _customerGenerator.Clone().RuleFor(x => x.Email, invalidEmail ).Generate();
        
        var response = await _client.PostAsJsonAsync("customers", customer);
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error!.Title.Should().Be("One or more validation errors occurred.");
        error!.Errors["Email"][0].Should().Be($"{invalidEmail} is not a valid email address");
    }
    
    [Fact]
    public async Task Create_ReturnsValidationError_WhenGitHubUserDoesNotExists()
    {
        const string invalidGitHubUser = "asdfasfs";
        var customer = _customerGenerator.Clone().RuleFor(x => x.GitHubUsername, invalidGitHubUser ).Generate();
        
        var response = await _client.PostAsJsonAsync("customers", customer);
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error!.Title.Should().Be("One or more validation errors occurred.");
        error!.Errors["Customer"][0].Should().Be($"There is no GitHub user with username {invalidGitHubUser}");
    }
    
    
    [Fact]
    public async Task Create_ReturnsInternalServerError_WhenGithubIsThrottled()
    {
        // Arrange
        var customer = _customerGenerator.Clone()
            .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ThrottledUser)
            .Generate();

        // Act
        var response = await _client.PostAsJsonAsync("customers", customer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}