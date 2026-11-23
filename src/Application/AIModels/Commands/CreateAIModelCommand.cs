using Application.AIModels.Exceptions;
using Application.Common.Interfaces.Repositories;
using Domain.AIModel;
using LanguageExt;
using MediatR;

namespace Application.AIModels.Commands;

public record CreateAIModelCommand : IRequest<Either<AIModelException, AIModel>>
{
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public ModelType Type { get; init; }
    public decimal ParametersBillions { get; init; }
    public DateTime ReleaseDate { get; init; }
}

public class CreateAIModelCommandHandler(IAIModelRepository aiModelRepository)
    : IRequestHandler<CreateAIModelCommand, Either<AIModelException, AIModel>>
{
    public async Task<Either<AIModelException, AIModel>> Handle(
        CreateAIModelCommand request,
        CancellationToken cancellationToken)
    {
        var existingModel = await aiModelRepository.GetByName(request.Name.Trim(), cancellationToken);

        return await existingModel.MatchAsync(
            model => Task.FromResult<Either<AIModelException, AIModel>>(
                new AIModelAlreadyExistsException(model.Id)),
            () => CreateEntity(request, cancellationToken));
    }

    private async Task<Either<AIModelException, AIModel>> CreateEntity(
        CreateAIModelCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = AIModel.New(
                AIModelId.New(),
                request.Name,
                request.Provider,
                request.Type,
                request.ParametersBillions,
                request.ReleaseDate);

            return await aiModelRepository.Add(model, cancellationToken);
        }
        catch (Exception exception)
        {
            return new AIModelUnhandledException(AIModelId.Empty(), exception);
        }
    }
}
