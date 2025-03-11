namespace PowerPad.Core.Contracts
{
    public interface IEditorContract
    {
        string GetContent();
        void SetContent(string content);
    }
}
