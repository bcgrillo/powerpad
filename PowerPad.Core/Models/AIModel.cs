namespace PowerPad.Core.Models
{
    public record AIModel
    (
        string Name,

        ModelProvider ModelProvider,

        long? Size = null,

        ModelStatus Status = ModelStatus.Unkown
    );
}