using Customers.Api.Database;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Testcontainers.PostgreSql;

namespace Customers.Api.Tests.Integration;

public class CustomerApiFactory : WebApplicationFactory<IAPIMarker>, IAsyncLifetime
{
    public const string ValidGithubUser = "validuser";
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder().Build();
    private readonly GithubApiServer _githubApiServer = new();


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => { logging.ClearProviders(); });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IDbConnectionFactory));
            services.AddSingleton<IDbConnectionFactory>(_ =>
                new NpgsqlConnectionFactory(
                    _dbContainer.GetConnectionString())
            );


            services.AddHttpClient("GitHub", httpClient =>
            {
                httpClient.BaseAddress = new Uri(_githubApiServer.Url);
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Accept, "application/vnd.github.v3+json");
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.UserAgent, $"Course-{Environment.MachineName}");
            });
        });
    }


    public async Task InitializeAsync()
    {
        _githubApiServer.Start();
        _githubApiServer.SetupUser(ValidGithubUser);
        await _dbContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        _githubApiServer.Dispose();
    }
}