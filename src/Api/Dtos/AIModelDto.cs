using Domain.AIModel;

namespace Api.Dtos;

public record AIModelDto(
    Guid Id,
    string Name,
    string Provider,
    ModelType Type,
    decimal ParametersBillions,
    bool IsActive,
    DateTime ReleaseDate,
    DateTime CreatedAt)
{
    public static AIModelDto FromDomainModel(AIModel model)
        => new(
            model.Id.Value,
            model.Name,
            model.Provider,
            model.Type,
            model.ParametersBillions,
            model.IsActive,
            model.ReleaseDate,
            model.CreatedAt);
}

public record CreateAIModelDto
{
    public required string Name { get; init; }
    public required string Provider { get; init; }
    public ModelType Type { get; init; }
    public decimal ParametersBillions { get; init; }
    public DateTime ReleaseDate { get; init; }
}

public record UpdateAIModelDto
{
    public required string Name { get; init; }
    public required string Provider { get; init; }
    public ModelType Type { get; init; }
    public decimal ParametersBillions { get; init; }
    public DateTime ReleaseDate { get; init; }
}

public record SearchAIModelDto
{
    public string? Name { get; init; }
    public string? Provider { get; init; }
    public ModelType? Type { get; init; }
    public decimal? ParametersBillionsFrom { get; init; }
    public decimal? ParametersBillionsTo { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? ReleaseDateFrom { get; init; }
    public DateTime? ReleaseDateTo { get; init; }
    public DateTime? CreatedAtFrom { get; init; }
    public DateTime? CreatedAtTo { get; init; }
}
