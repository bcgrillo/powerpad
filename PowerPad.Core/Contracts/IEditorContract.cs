namespace PowerPad.Core.Contracts
{
    public interface IEditorContract
    {
        string GetContent(bool plainText = false);
        void SetContent(string content);
        int WordCount();
    }
}
