namespace PowerPad.Core.Contracts
{
    /// <summary>
    /// Defines the contract for an editor, providing methods to get, set, and analyze content.
    /// </summary>
    public interface IEditorContract
    {
        /// <summary>
        /// Retrieves the content of the editor.
        /// </summary>
        /// <param name="plainText">If true, returns the content as plain text without formatting.</param>
        /// <returns>The content of the editor as a string.</returns>
        string GetContent(bool plainText = false);

        /// <summary>
        /// Sets the content of the editor.
        /// </summary>
        /// <param name="content">The content to set in the editor.</param>
        void SetContent(string content);

        /// <summary>
        /// Calculates the number of words in the editor's content.
        /// </summary>
        /// <returns>The word count as an integer.</returns>
        int WordCount();
    }
}