using Domain.AIModel;

namespace Tests.Data;

public static class AIModelsTestData
{
    public static AIModel FirstTestModel() => AIModel.New(
        AIModelId.New(),
        "GLM-5.3-Flash",
        "Z.ai",
        ModelType.Chat,
        35m,
        new DateTime(2026, 8, 26, 0, 0, 0, DateTimeKind.Utc));

    public static AIModel SecondTestModel()
    {
        var model = AIModel.New(
            AIModelId.New(),
            "FLUX 3 Image Klein",
            "Black Forest Labs",
            ModelType.Image,
            12m,
            new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc));

        model.Deactivate();

        return model;
    }

    public static AIModel ThirdTestModel() => AIModel.New(
        AIModelId.New(),
        "DeepSeek V4.1 Flash 1017",
        "DeepSeek",
        ModelType.Chat,
        1.017m,
        new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));

    public static AIModel FourthTestModel()
    {
        var model = AIModel.New(
            AIModelId.New(),
            "Claude Fable 5.2",
            "Anthropic",
            ModelType.Chat,
            400m,
            new DateTime(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc));

        model.Deactivate();

        return model;
    }
}
