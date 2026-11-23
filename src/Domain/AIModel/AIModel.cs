namespace Domain.AIModel;

public class AIModel
{
    public AIModelId Id { get; }
    public string Name { get; private set; }
    public string Provider { get; private set; }
    public ModelType Type { get; private set; }
    public decimal ParametersBillions { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime ReleaseDate { get; private set; }
    public DateTime CreatedAt { get; }

    private AIModel(
        AIModelId id,
        string name,
        string provider,
        ModelType type,
        decimal parametersBillions,
        DateTime releaseDate,
        DateTime createdAt)
        => (Id, Name, Provider, Type, ParametersBillions, IsActive, ReleaseDate, CreatedAt) =
            (id, name, provider, type, parametersBillions, true, releaseDate, createdAt);

    public static AIModel New(
        AIModelId id,
        string name,
        string provider,
        ModelType type,
        decimal parametersBillions,
        DateTime releaseDate)
        => new(id, name.Trim(), provider.Trim(), type, parametersBillions, releaseDate, DateTime.UtcNow);

    public void UpdateName(string name)
    {
        Name = name.Trim();
    }

    public void UpdateProvider(string provider)
    {
        Provider = provider.Trim();
    }

    public void UpdateType(ModelType type)
    {
        Type = type;
    }

    public void UpdateParameters(decimal parametersBillions)
    {
        ParametersBillions = parametersBillions;
    }

    public void UpdateReleaseDate(DateTime releaseDate)
    {
        ReleaseDate = releaseDate;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
