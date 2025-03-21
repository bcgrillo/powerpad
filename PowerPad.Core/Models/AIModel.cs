namespace PowerPad.Core.Models
{
    public class AIModel
    {
        public string Name { get; private set; }

        public ModelStatus Status { get; internal set; } = ModelStatus.Unkown;

        public ModelProvider ModelProvider { get; private set; }

        public AIModel(string name, ModelProvider modelProvider)
        {
            Name = name;
            ModelProvider = modelProvider;
        }
    }
}