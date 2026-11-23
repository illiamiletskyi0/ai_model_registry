using Api.Dtos;
using Api.Modules.Errors;
using Application.AIModels.Commands;
using Application.Common.Interfaces.Queries;
using Domain.AIModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("aimodels")]
public class AIModelsController(ISender sender, IAIModelQueries aiModelQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AIModelDto>>> GetAll(CancellationToken cancellationToken)
    {
        var models = await aiModelQueries.GetAll(cancellationToken);

        return Ok(models.Select(AIModelDto.FromDomainModel).ToList());
    }
    

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AIModelDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var model = await aiModelQueries.GetById(new AIModelId(id), cancellationToken);

        return model.Match<ActionResult<AIModelDto>>(
            m => Ok(AIModelDto.FromDomainModel(m)),
            () => NotFound());
    }

    [HttpPost("filter")]
    public async Task<ActionResult<IReadOnlyList<AIModelDto>>> Filter(
        [FromBody] SearchAIModelDto dto,
        CancellationToken cancellationToken)
    {
        var models = await aiModelQueries.Filter(
            dto.Name,
            dto.Provider,
            dto.Type,
            dto.ParametersBillionsFrom,
            dto.ParametersBillionsTo,
            dto.IsActive,
            dto.ReleaseDateFrom,
            dto.ReleaseDateTo,
            dto.CreatedAtFrom,
            dto.CreatedAtTo,
            cancellationToken);

        return Ok(models.Select(AIModelDto.FromDomainModel).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AIModelDto>> Create(
        [FromBody] CreateAIModelDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateAIModelCommand
        {
            Name = dto.Name,
            Provider = dto.Provider,
            Type = dto.Type,
            ParametersBillions = dto.ParametersBillions,
            ReleaseDate = dto.ReleaseDate
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<AIModelDto>>(
            model => CreatedAtAction(
                nameof(GetById),
                new { id = model.Id.Value },
                AIModelDto.FromDomainModel(model)),
            error => error.ToObjectResult());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AIModelDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateAIModelDto dto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAIModelCommand
        {
            AIModelId = id,
            Name = dto.Name,
            Provider = dto.Provider,
            Type = dto.Type,
            ParametersBillions = dto.ParametersBillions,
            ReleaseDate = dto.ReleaseDate
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<AIModelDto>>(
            model => Ok(AIModelDto.FromDomainModel(model)),
            error => error.ToObjectResult());
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAIModelCommand
        {
            AIModelId = id
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult>(
            _ => NoContent(),
            error => error.ToObjectResult());
    }
}
