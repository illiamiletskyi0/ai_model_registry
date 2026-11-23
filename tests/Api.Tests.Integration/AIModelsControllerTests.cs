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

public class AIModelsControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly AIModel _firstModel;
    private readonly AIModel _secondModel;
    private readonly AIModel _thirdModel;
    private readonly AIModel _fourthModel;

    public AIModelsControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _firstModel = AIModelsTestData.FirstTestModel();
        _secondModel = AIModelsTestData.SecondTestModel();
        _thirdModel = AIModelsTestData.ThirdTestModel();
        _fourthModel = AIModelsTestData.FourthTestModel();
    }

    public async Task InitializeAsync()
    {
        await Context.AIModels.AddRangeAsync(_firstModel, _secondModel, _thirdModel, _fourthModel);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        var models = await Context.AIModels.ToListAsync();
        Context.AIModels.RemoveRange(models);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAndPersistsModel()
    {
        // Arrange
        var request = new CreateAIModelDto
        {
            Name = "Mistral Large 3",
            Provider = "Mistral AI",
            Type = ModelType.Chat,
            ParametersBillions = 123m,
            ReleaseDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/aimodels", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdDto = await response.Content.ReadFromJsonAsync<AIModelDto>(JsonOptions);
        createdDto.Should().NotBeNull();
        createdDto!.Id.Should().NotBeEmpty();

        var persisted = await GetModel(new AIModelId(createdDto.Id));
        persisted.Name.Should().Be(request.Name);
        persisted.Provider.Should().Be(request.Provider);
        persisted.Type.Should().Be(request.Type);
        persisted.ParametersBillions.Should().Be(request.ParametersBillions);
        persisted.ReleaseDate.Should().Be(request.ReleaseDate);
        persisted.IsActive.Should().BeTrue();
        persisted.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsConflictAndKeepsExistingModel()
    {
        // Arrange
        var request = new CreateAIModelDto
        {
            Name = _firstModel.Name,
            Provider = "Duplicate Provider",
            Type = ModelType.Chat,
            ParametersBillions = 10m,
            ReleaseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/aimodels", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var modelsWithSameName = await Context.AIModels
            .AsNoTracking()
            .Where(model => model.Name == request.Name)
            .ToListAsync();

        modelsWithSameName.Should().ContainSingle();
        modelsWithSameName[0].Id.Should().Be(_firstModel.Id);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequestAndDoesNotPersistModel()
    {
        // Arrange
        var request = new CreateAIModelDto
        {
            Name = string.Empty,
            Provider = "Empty Name Provider",
            Type = ModelType.Chat,
            ParametersBillions = 5m,
            ReleaseDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/aimodels", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Context.AIModels.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task Create_WithTooLongName_ReturnsBadRequestAndDoesNotPersistModel()
    {
        // Arrange
        var request = new CreateAIModelDto
        {
            Name = new string('x', 201),
            Provider = "Too Long Name Provider",
            Type = ModelType.Chat,
            ParametersBillions = 5m,
            ReleaseDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/aimodels", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Context.AIModels.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task Create_WithNegativeParameters_ReturnsBadRequestAndDoesNotPersistModel()
    {
        // Arrange
        var request = new CreateAIModelDto
        {
            Name = "Negative Parameters Model",
            Provider = "Negative Provider",
            Type = ModelType.Chat,
            ParametersBillions = -1m,
            ReleaseDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/aimodels", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Context.AIModels.AnyAsync(model => model.Name == request.Name)).Should().BeFalse();
        (await Context.AIModels.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var request = new UpdateAIModelDto
        {
            Name = "GLM-5.3-Flash Turbo",
            Provider = "Z.ai Labs",
            Type = ModelType.Multimodal,
            ParametersBillions = 36m,
            ReleaseDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/aimodels/{_firstModel.Id.Value}",
            request,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedDto = await response.Content.ReadFromJsonAsync<AIModelDto>(JsonOptions);
        updatedDto.Should().NotBeNull();
        updatedDto!.Id.Should().Be(_firstModel.Id.Value);
        updatedDto.Name.Should().Be(request.Name);
        updatedDto.Provider.Should().Be(request.Provider);
        updatedDto.Type.Should().Be(request.Type);
        updatedDto.ParametersBillions.Should().Be(request.ParametersBillions);
        updatedDto.ReleaseDate.Should().Be(request.ReleaseDate);

        var persisted = await GetModel(_firstModel.Id);
        persisted.Name.Should().Be(request.Name);
        persisted.Provider.Should().Be(request.Provider);
        persisted.Type.Should().Be(request.Type);
        persisted.ParametersBillions.Should().Be(request.ParametersBillions);
        persisted.ReleaseDate.Should().Be(request.ReleaseDate);

        persisted.Name.Should().NotBe(_firstModel.Name);
        persisted.Provider.Should().NotBe(_firstModel.Provider);
        persisted.Type.Should().NotBe(_firstModel.Type);
        persisted.ParametersBillions.Should().NotBe(_firstModel.ParametersBillions);
        persisted.ReleaseDate.Should().NotBe(_firstModel.ReleaseDate);
    }

    [Fact]
    public async Task Update_WithSameName_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var request = new UpdateAIModelDto
        {
            Name = _secondModel.Name,
            Provider = "Black Forest Labs Updated",
            Type = ModelType.Image,
            ParametersBillions = 13m,
            ReleaseDate = new DateTime(2026, 10, 16, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/aimodels/{_secondModel.Id.Value}",
            request,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var persisted = await GetModel(_secondModel.Id);
        persisted.Name.Should().Be(_secondModel.Name);
        persisted.Provider.Should().Be(request.Provider);
        persisted.Type.Should().Be(request.Type);
        persisted.ParametersBillions.Should().Be(request.ParametersBillions);
        persisted.ReleaseDate.Should().Be(request.ReleaseDate);
    }

    [Fact]
    public async Task Update_WithNameOfAnotherModel_ReturnsConflictAndDoesNotChangeModel()
    {
        // Arrange
        var request = new UpdateAIModelDto
        {
            Name = _fourthModel.Name,
            Provider = "Conflicting Provider",
            Type = ModelType.Chat,
            ParametersBillions = 5m,
            ReleaseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/aimodels/{_firstModel.Id.Value}",
            request,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var persisted = await GetModel(_firstModel.Id);
        persisted.Should().BeEquivalentTo(_firstModel, options => options.Excluding(model => model.CreatedAt));
    }

    [Fact]
    public async Task Update_NonExistingModel_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var request = new UpdateAIModelDto
        {
            Name = "Ghost Model",
            Provider = "Ghost Provider",
            Type = ModelType.Chat,
            ParametersBillions = 1m,
            ReleaseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/aimodels/{nonExistingId}",
            request,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Context.AIModels.AnyAsync(model => model.Id == new AIModelId(nonExistingId))).Should().BeFalse();
        (await Context.AIModels.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task Update_WithInvalidData_ReturnsBadRequestAndDoesNotChangeModel()
    {
        // Arrange
        var request = new UpdateAIModelDto
        {
            Name = new string('x', 201),
            Provider = "Invalid Update Provider",
            Type = ModelType.Chat,
            ParametersBillions = -1m,
            ReleaseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/aimodels/{_thirdModel.Id.Value}",
            request,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var persisted = await GetModel(_thirdModel.Id);
        persisted.Should().BeEquivalentTo(_thirdModel, options => options.Excluding(model => model.CreatedAt));
    }

    [Fact]
    public async Task Delete_ExistingModel_ReturnsNoContentAndRemovesOnlyThatModel()
    {
        // Act
        var response = await Client.DeleteAsync($"/aimodels/{_fourthModel.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Context.AIModels.AnyAsync(model => model.Id == _fourthModel.Id)).Should().BeFalse();
        (await Context.AIModels.CountAsync()).Should().Be(3);

        var remainingIds = await Context.AIModels
            .AsNoTracking()
            .Select(model => model.Id.Value)
            .ToListAsync();

        remainingIds.Should().BeEquivalentTo(new[]
        {
            _firstModel.Id.Value,
            _secondModel.Id.Value,
            _thirdModel.Id.Value
        });
    }

    [Fact]
    public async Task Delete_NonExistingModel_ReturnsNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"/aimodels/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Context.AIModels.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task Delete_AlreadyDeletedModel_ReturnsNotFound()
    {
        // Act
        var firstResponse = await Client.DeleteAsync($"/aimodels/{_firstModel.Id.Value}");
        var secondResponse = await Client.DeleteAsync($"/aimodels/{_firstModel.Id.Value}");

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Context.AIModels.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task GetAll_ReturnsAllSeededModels()
    {
        // Act
        var response = await Client.GetAsync("/aimodels");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var models = await response.Content.ReadFromJsonAsync<List<AIModelDto>>(JsonOptions);
        models.Should().NotBeNull();
        models!.Should().HaveCount(4);
        models
            .Select(model => model.Id)
            .Should()
            .BeEquivalentTo(new[]
            {
                _firstModel.Id.Value,
                _secondModel.Id.Value,
                _thirdModel.Id.Value,
                _fourthModel.Id.Value
            });
    }

    [Fact]
    public async Task GetById_ReturnsModelFromDatabase()
    {
        // Act
        var response = await Client.GetAsync($"/aimodels/{_thirdModel.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var modelDto = await response.Content.ReadFromJsonAsync<AIModelDto>(JsonOptions);
        modelDto.Should().NotBeNull();

        var persisted = await GetModel(_thirdModel.Id);
        modelDto.Should().Be(AIModelDto.FromDomainModel(persisted));
    }

    [Fact]
    public async Task GetById_NonExistingModel_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/aimodels/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<AIModel> GetModel(AIModelId id)
    {
        var model = await Context.AIModels
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        model.Should().NotBeNull();

        return model!;
    }
}
