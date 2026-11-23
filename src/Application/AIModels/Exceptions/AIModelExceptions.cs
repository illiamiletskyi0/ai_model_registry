using Domain.AIModel;

namespace Application.AIModels.Exceptions;

public abstract class AIModelException(AIModelId modelId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public AIModelId AIModelId { get; } = modelId;
}

public class AIModelAlreadyExistsException(AIModelId modelId)
    : AIModelException(modelId, $"AI model already exists under id {modelId}");

public class AIModelNotFoundException(AIModelId modelId)
    : AIModelException(modelId, $"AI model not found under id {modelId}");

public class AIModelUnhandledException(AIModelId modelId, Exception? innerException = null)
    : AIModelException(modelId, $"AI model unhandled exception under id {modelId}", innerException);
