using FluentValidation;

namespace Application.AIModels.Commands;

public class UpdateAIModelCommandValidator : AbstractValidator<UpdateAIModelCommand>
{
    public UpdateAIModelCommandValidator()
    {
        RuleFor(x => x.AIModelId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(200);
        RuleFor(x => x.Provider).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.ParametersBillions).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReleaseDate).NotEmpty();
    }
}
