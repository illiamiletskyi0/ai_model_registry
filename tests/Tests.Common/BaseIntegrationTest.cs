using System.Net.Http.Headers;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Tests.Common.Infrastructure;
using Xunit;

namespace Tests.Common;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebFactory>
{
    protected BaseIntegrationTest(IntegrationTestWebFactory factory)
    {
        Factory = factory;

        var authenticatedFactory = factory.WithWebHostBuilderMock();

        Client = authenticatedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.SchemeName);

        Context = authenticatedFactory.Services
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
    }

    protected IntegrationTestWebFactory Factory { get; }
    protected ApplicationDbContext Context { get; }
    protected HttpClient Client { get; }
}
