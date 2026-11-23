using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.AIModel;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AIModelRepository(ApplicationDbContext context) : IAIModelRepository, IAIModelQueries
{
    public async Task<IReadOnlyList<AIModel>> GetAll(CancellationToken cancellationToken)
    {
        return await context.AIModels.ToListAsync(cancellationToken);
    }

    public async Task<Option<AIModel>> GetByName(string name, CancellationToken cancellationToken)
    {
        var entity = await context.AIModels
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

        return entity is null ? Option<AIModel>.None : entity;
    }

    public async Task<Option<AIModel>> GetById(AIModelId id, CancellationToken cancellationToken)
    {
        var entity = await context.AIModels
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? Option<AIModel>.None : entity;
    }

    public async Task<IReadOnlyList<AIModel>> Filter(
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
        CancellationToken cancellationToken)
    {
        var query = context.AIModels.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var pattern = $"%{name.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(provider))
        {
            var pattern = $"%{provider.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Provider, pattern));
        }

        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }

        if (parametersBillionsFrom.HasValue)
        {
            query = query.Where(x => x.ParametersBillions >= parametersBillionsFrom.Value);
        }

        if (parametersBillionsTo.HasValue)
        {
            query = query.Where(x => x.ParametersBillions <= parametersBillionsTo.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (releaseDateFrom.HasValue)
        {
            var from = AsUtc(releaseDateFrom.Value);
            query = query.Where(x => x.ReleaseDate >= from);
        }

        if (releaseDateTo.HasValue)
        {
            var to = AsUtc(releaseDateTo.Value);
            query = query.Where(x => x.ReleaseDate <= to);
        }

        if (createdAtFrom.HasValue)
        {
            var from = AsUtc(createdAtFrom.Value);
            query = query.Where(x => x.CreatedAt >= from);
        }

        if (createdAtTo.HasValue)
        {
            var to = AsUtc(createdAtTo.Value);
            query = query.Where(x => x.CreatedAt <= to);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<AIModel> Add(AIModel model, CancellationToken cancellationToken)
    {
        await context.AIModels.AddAsync(model, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return model;
    }

    public async Task<AIModel> Update(AIModel model, CancellationToken cancellationToken)
    {
        context.AIModels.Update(model);
        await context.SaveChangesAsync(cancellationToken);

        return model;
    }

    public async Task Delete(AIModel model, CancellationToken cancellationToken)
    {
        context.AIModels.Remove(model);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
