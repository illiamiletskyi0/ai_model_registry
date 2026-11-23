using Application.AIModels.Exceptions;
using Application.Common.Interfaces.Repositories;
using Domain.AIModel;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace Application.AIModels.Commands;

public record UpdateAIModelCommand : IRequest<Either<AIModelException, AIModel>>
{
    public required Guid AIModelId { get; init; }
    public required string Name { get; init; }
    public required string Provider { get; init; }
    public ModelType Type { get; init; }
    public decimal ParametersBillions { get; init; }
    public DateTime ReleaseDate { get; init; }
}

public class UpdateAIModelCommandHandler(IAIModelRepository aiModelRepository)
    : IRequestHandler<UpdateAIModelCommand, Either<AIModelException, AIModel>>
{
    public async Task<Either<AIModelException, AIModel>> Handle(
        UpdateAIModelCommand request,
        CancellationToken cancellationToken)
    {
        var modelId = new AIModelId(request.AIModelId);

        var model = await aiModelRepository.GetById(modelId, cancellationToken);
        return await model.MatchAsync(
            m => CheckDuplicates(m.Id, request.Name, cancellationToken)
                .BindAsync(_ => UpdateEntity(m, request, cancellationToken)),
            () => new AIModelNotFoundException(modelId));
    }

    private async Task<Either<AIModelException, AIModel>> UpdateEntity(
        AIModel model,
        UpdateAIModelCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            model.UpdateName(request.Name);
            model.UpdateProvider(request.Provider);
            model.UpdateType(request.Type);
            model.UpdateParameters(request.ParametersBillions);
            model.UpdateReleaseDate(request.ReleaseDate);

            return await aiModelRepository.Update(model, cancellationToken);
        }
        catch (Exception exception)
        {
            return new AIModelUnhandledException(model.Id, exception);
        }
    }

    private async Task<Either<AIModelException, Unit>> CheckDuplicates(
        AIModelId currentAIModelId,
        string name,
        CancellationToken cancellationToken)
    {
        var model = await aiModelRepository.GetByName(name.Trim(), cancellationToken);

        return model.Match<Either<AIModelException, Unit>>(
            m => m.Id == currentAIModelId ? Unit.Default : new AIModelAlreadyExistsException(m.Id),
            () => Unit.Default);
    }
}
