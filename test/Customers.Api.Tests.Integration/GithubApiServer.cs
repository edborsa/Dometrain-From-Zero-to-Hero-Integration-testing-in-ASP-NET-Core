using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Customers.Api.Tests.Integration;

public class GithubApiServer : IDisposable
{
    private WireMockServer _server;
    public string Url => _server.Url!;

    public void Start()
    {
        _server = WireMockServer.Start();
    }

    public void SetupUser(string username)
    {
        _server.Given(
            Request.Create()
                .WithPath($"/users/{username}")
                .UsingGet()
        ).RespondWith(
            Response.Create()
                .WithBody(GeneratePubUserResponseBody(username))
                .WithHeader("content-type", "application/json; charset=utf-8")
                .WithStatusCode(200)
        );
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    private static string GeneratePubUserResponseBody(string username)
    {
        return $@"
                {{
                    login: ""{username}"",
                    id: 67104228,
                    node_id: ""MDQ6VXNlcjY3MTA0MjI4"",
                    avatar_url: ""https://avatars.githubusercontent.com/u/67104228?v=4"",
                    gravatar_id: """",
                    url: ""https://api.github.com/users/{username}"",
                    html_url: ""https://github.com/{username}"",
                    followers_url: ""https://api.github.com/users/{username}/followers"",
                    following_url: ""https://api.github.com/users/{username}/following{{/other_user}}"",
                    gists_url: ""https://api.github.com/users/{username}/gists{{/gist_id}}"",
                    starred_url: ""https://api.github.com/users/{username}/starred{{/owner}}{{/repo}}"",
                    subscriptions_url: ""https://api.github.com/users/{username}/subscriptions"",
                    organizations_url: ""https://api.github.com/users/{username}/orgs"",
                    repos_url: ""https://api.github.com/users/{username}/repos"",
                    events_url: ""https://api.github.com/users/{username}/events{{/privacy}}"",
                    received_events_url: ""https://api.github.com/users/{username}/received_events"",
                    type: ""User"",
                    site_admin: false,
                    name: null,
                    company: null,
                    blog: """",
                    location: null,
                    email: null,
                    hireable: null,
                    bio: null,
                    twitter_username: null,
                    public_repos: 0,
                    public_gists: 0,
                    followers: 2,
                    following: 0,
                    created_at: ""2020-06-18T11:47:58Z"",
                    updated_at: ""2020-06-18T11:47:58Z""
                }}";
    }
}