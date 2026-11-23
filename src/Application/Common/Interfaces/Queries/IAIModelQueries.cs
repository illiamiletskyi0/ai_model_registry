using Domain.AIModel;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IAIModelQueries
{
    Task<IReadOnlyList<AIModel>> GetAll(CancellationToken cancellationToken);
    Task<Option<AIModel>> GetById(AIModelId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AIModel>> Filter(
        string? name,
        string? provider,
        ModelType? type,
        decimal? parametersBillionsFrom,
        decimal? parametersBillionsTo,
        bool? isActive,
        DateTime? releaseDateFrom,
        DateTime? releaseDateTo,
        DateTime? createdAtFrom,
        DateTime? createdAtTo,
        CancellationToken cancellationToken);
}
