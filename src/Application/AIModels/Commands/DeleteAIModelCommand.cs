using Application.AIModels.Exceptions;
using Application.Common.Interfaces.Repositories;
using Domain.AIModel;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace Application.AIModels.Commands;

public record DeleteAIModelCommand : IRequest<Either<AIModelException, Unit>>
{
    public required Guid AIModelId { get; init; }
}

public class DeleteAIModelCommandHandler(IAIModelRepository aiModelRepository)
    : IRequestHandler<DeleteAIModelCommand, Either<AIModelException, Unit>>
{
    public async Task<Either<AIModelException, Unit>> Handle(
        DeleteAIModelCommand request,
        CancellationToken cancellationToken)
    {
        var modelId = new AIModelId(request.AIModelId);

        var model = await aiModelRepository.GetById(modelId, cancellationToken);

        return await model.MatchAsync(
            m => DeleteEntity(m, cancellationToken),
            () => new AIModelNotFoundException(modelId));
    }

    private async Task<Either<AIModelException, Unit>> DeleteEntity(
        AIModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            await aiModelRepository.Delete(model, cancellationToken);

            return Unit.Default;
        }
        catch (Exception exception)
        {
            return new AIModelUnhandledException(model.Id, exception);
        }
    }
}
