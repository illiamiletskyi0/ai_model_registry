namespace Domain.AIModel;

public record AIModelId(Guid Value)
{
    public static AIModelId Empty() => new(Guid.Empty);
    public static AIModelId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
