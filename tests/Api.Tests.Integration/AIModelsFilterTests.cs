using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Dtos;
using Domain.AIModel;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Common.Infrastructure;
using Tests.Data;
using Xunit;

namespace Api.Tests.Integration;

public class AIModelsFilterTests : BaseIntegrationTest, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AIModel _firstModel;
    private readonly AIModel _secondModel;
    private readonly AIModel _thirdModel;
    private readonly AIModel _fourthModel;

    private IReadOnlyDictionary<AIModelId, DateTime> _createdAtByModelId =
        new Dictionary<AIModelId, DateTime>();

    public AIModelsFilterTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _firstModel = AIModelsTestData.FirstTestModel();
        Thread.Sleep(2);
        _secondModel = AIModelsTestData.SecondTestModel();
        Thread.Sleep(2);
        _thirdModel = AIModelsTestData.ThirdTestModel();
        Thread.Sleep(2);
        _fourthModel = AIModelsTestData.FourthTestModel();
    }

    public async Task InitializeAsync()
    {
        await Context.AIModels.AddRangeAsync(_firstModel, _secondModel, _thirdModel, _fourthModel);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        _createdAtByModelId = await Context.AIModels
            .AsNoTracking()
            .ToDictionaryAsync(model => model.Id, model => model.CreatedAt);
    }

    public async Task DisposeAsync()
    {
        var models = await Context.AIModels.ToListAsync();
        Context.AIModels.RemoveRange(models);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task Filter_ByName_ReturnsPartialMatches()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto { Name = "Flash" });

        // Assert
        AssertFilterResult(result, new[]
        {
            _firstModel.Id.Value,
            _thirdModel.Id.Value
        });
    }

    [Fact]
    public async Task Filter_ByProvider_ReturnsPartialMatches()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto { Provider = "Deep" });

        // Assert
        AssertFilterResult(result, new[] { _thirdModel.Id.Value });
    }

    [Fact]
    public async Task Filter_ByType_ReturnsExactMatches()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto { Type = ModelType.Chat });

        // Assert
        AssertFilterResult(result, new[]
        {
            _firstModel.Id.Value,
            _thirdModel.Id.Value,
            _fourthModel.Id.Value
        });
    }

    [Fact]
    public async Task Filter_ByParametersFrom_ReturnsModelsAtOrAboveBoundary()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto { ParametersBillionsFrom = 12m });

        // Assert
        AssertFilterResult(result, new[]
        {
            _firstModel.Id.Value,
            _secondModel.Id.Value,
            _fourthModel.Id.Value
        });
    }

    [Fact]
    public async Task Filter_ByParametersTo_ReturnsModelsAtOrBelowBoundary()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto { ParametersBillionsTo = 35m });

        // Assert
        AssertFilterResult(result, new[]
        {
            _firstModel.Id.Value,
            _secondModel.Id.Value,
            _thirdModel.Id.Value
        });
    }

    [Fact]
    public async Task Filter_ByReleaseDateFrom_ReturnsModelsAtOrAfterBoundary()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto
        {
            ReleaseDateFrom = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc)
        });

        // Assert
        AssertFilterResult(result, new[]
        {
            _secondModel.Id.Value,
            _thirdModel.Id.Value,
            _fourthModel.Id.Value
        });
    }

    [Fact]
    public async Task Filter_ByReleaseDateTo_ReturnsModelsAtOrBeforeBoundary()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto
        {
            ReleaseDateTo = new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc)
        });

        // Assert
        AssertFilterResult(result, new[]
        {
            _firstModel.Id.Value,
            _secondModel.Id.Value,
            _thirdModel.Id.Value
        });
    }

    [Fact]
    public async Task Filter_ByIsActive_ReturnsOnlyActiveModels()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto { IsActive = true });

        // Assert
        AssertFilterResult(result, new[]
        {
            _firstModel.Id.Value,
            _thirdModel.Id.Value
        });
    }

    [Fact]
    public async Task Filter_ByCreatedAtFrom_ReturnsModelsCreatedAtOrAfterBoundary()
    {
        // Arrange
        var boundary = GetDistinctCreatedAtValues()[1];
        var expectedIds = _createdAtByModelId
            .Where(entry => entry.Value >= boundary)
            .Select(entry => entry.Key.Value)
            .ToArray();

        // Act
        var result = await FilterAsync(new SearchAIModelDto { CreatedAtFrom = boundary });

        // Assert
        AssertFilterResult(result, expectedIds);
    }

    [Fact]
    public async Task Filter_ByCreatedAtTo_ReturnsModelsCreatedAtOrBeforeBoundary()
    {
        // Arrange
        var boundary = GetDistinctCreatedAtValues()[^2];
        var expectedIds = _createdAtByModelId
            .Where(entry => entry.Value <= boundary)
            .Select(entry => entry.Key.Value)
            .ToArray();

        // Act
        var result = await FilterAsync(new SearchAIModelDto { CreatedAtTo = boundary });

        // Assert
        AssertFilterResult(result, expectedIds);
    }

    [Fact]
    public async Task Filter_ByName_IsCaseInsensitive()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto { Name = "fLaSh" });

        // Assert
        AssertFilterResult(result, new[]
        {
            _firstModel.Id.Value,
            _thirdModel.Id.Value
        });
    }

    [Fact]
    public async Task Filter_ByParametersRange_BoundariesAreInclusive()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto
        {
            ParametersBillionsFrom = 35m,
            ParametersBillionsTo = 35m
        });

        // Assert
        AssertFilterResult(result, new[] { _firstModel.Id.Value });
    }

    [Fact]
    public async Task Filter_ByReleaseDateRange_BoundariesAreInclusive()
    {
        // Arrange
        var boundary = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await FilterAsync(new SearchAIModelDto
        {
            ReleaseDateFrom = boundary,
            ReleaseDateTo = boundary
        });

        // Assert
        AssertFilterResult(result, new[] { _secondModel.Id.Value });
    }

    [Fact]
    public async Task Filter_ByParametersRange_WithBothBoundaries_ReturnsModelsWithinRange()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto
        {
            ParametersBillionsFrom = 12m,
            ParametersBillionsTo = 35m
        });

        // Assert
        AssertFilterResult(result, new[]
        {
            _firstModel.Id.Value,
            _secondModel.Id.Value
        });
    }

    [Fact]
    public async Task Filter_ByCombinationOfParameters_ReturnsModelsMatchingAllConditions()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto
        {
            Provider = "a",
            Type = ModelType.Chat,
            IsActive = false
        });

        // Assert
        AssertFilterResult(result, new[] { _fourthModel.Id.Value });
    }

    [Fact]
    public async Task Filter_WithoutParameters_ReturnsAllModels()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto());

        // Assert
        AssertFilterResult(
            result,
            new[]
            {
                _firstModel.Id.Value,
                _secondModel.Id.Value,
                _thirdModel.Id.Value,
                _fourthModel.Id.Value
            },
            allowAll: true);
    }

    [Fact]
    public async Task Filter_WithNoMatchingParameters_ReturnsOkAndEmptyList()
    {
        // Act
        var result = await FilterAsync(new SearchAIModelDto { Name = "definitely-no-such-model" });

        // Assert
        AssertFilterResult(result, Array.Empty<Guid>(), allowEmpty: true);
    }

    private async Task<IReadOnlyList<AIModelDto>> FilterAsync(SearchAIModelDto filter)
    {
        var response = await Client.PostAsJsonAsync("/aimodels/filter", filter, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var models = await response.Content.ReadFromJsonAsync<List<AIModelDto>>(JsonOptions);
        models.Should().NotBeNull();

        return models!;
    }

    private static void AssertFilterResult(
        IReadOnlyCollection<AIModelDto> actual,
        IReadOnlyCollection<Guid> expectedIds,
        bool allowAll = false,
        bool allowEmpty = false)
    {
        actual.Select(model => model.Id).Should().BeEquivalentTo(expectedIds);

        if (!allowEmpty)
        {
            expectedIds.Should().NotBeEmpty();
        }

        if (!allowAll)
        {
            expectedIds.Count.Should().BeLessThan(4);
        }
    }

    private DateTime[] GetDistinctCreatedAtValues()
    {
        var values = _createdAtByModelId
            .Values
            .Distinct()
            .OrderBy(value => value)
            .ToArray();

        values.Should().HaveCountGreaterThan(1);

        return values;
    }
}
