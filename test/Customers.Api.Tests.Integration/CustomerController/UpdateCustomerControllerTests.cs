using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Customers.Api.Tests.Integration.CustomerController;

public class UpdateCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _client;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public UpdateCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task Update_UpdatesUser_WhenDataIsValid()
    {
        // ARRANGE
        var customer = _customerGenerator.Generate();
        var createdResponse = await _client.PostAsJsonAsync("customers", customer);
        var customerCreated = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        customer = _customerGenerator.Generate();
        
        // ACT 
        var updateCustomerResponse = await _client.PutAsJsonAsync($"customers/{customerCreated!.Id}", customer);
        var updatedCustomer = await updateCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        updateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedCustomer.Should().BeEquivalentTo(customer);
    }

    // [Fact]
    // public async Task Create_ReturnsValidationError_WhenEmailIsInvalid()
    // {
    //     const string invalidEmail = "asdfasfs";
    //     var customer = _customerGenerator.Clone().RuleFor(x => x.Email, invalidEmail).Generate();
    //
    //     var response = await _client.PostAsJsonAsync("customers", customer);
    //
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    //     error!.Status.Should().Be(400);
    //     error!.Title.Should().Be("One or more validation errors occurred.");
    //     error!.Errors["Email"][0].Should().Be($"{invalidEmail} is not a valid email address");
    // }

    [Fact]
    public async Task Create_ReturnsValidationError_WhenGitHubUserDoesNotExists()
    {
        const string invalidGitHubUser = "asdfasfs";
        var customer = _customerGenerator.Clone().RuleFor(x => x.GitHubUsername, invalidGitHubUser).Generate();

        var response = await _client.PostAsJsonAsync("customers", customer);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error!.Title.Should().Be("One or more validation errors occurred.");
        error!.Errors["Customer"][0].Should().Be($"There is no GitHub user with username {invalidGitHubUser}");
    }
}