using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Common.Infrastructure;
using Xunit;

namespace Api.Tests.Integration;

public class InfrastructureSmokeTests(IntegrationTestWebFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Api_starts_with_container_database()
    {
        var response = await Client.GetAsync("/aimodels");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Context.Database.GetConnectionString().Should().Be(Factory.DatabaseConnectionString);
        (await Context.AIModels.CountAsync()).Should().Be(0);
    }
}
