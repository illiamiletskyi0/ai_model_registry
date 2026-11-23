using Domain.AIModel;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IAIModelRepository
{
    Task<IReadOnlyList<AIModel>> GetAll(CancellationToken cancellationToken);
    Task<Option<AIModel>> GetByName(string name, CancellationToken cancellationToken);
    Task<Option<AIModel>> GetById(AIModelId id, CancellationToken cancellationToken);
    Task<AIModel> Add(AIModel model, CancellationToken cancellationToken);
    Task<AIModel> Update(AIModel model, CancellationToken cancellationToken);
    Task Delete(AIModel model, CancellationToken cancellationToken);
}
