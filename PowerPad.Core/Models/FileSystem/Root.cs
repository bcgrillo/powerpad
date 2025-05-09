namespace PowerPad.Core.Models.FileSystem
{
    /// <summary>
    /// Represents the root folder in the file system. 
    /// The root folder has a fixed path and does not have a parent folder.
    /// </summary>
    public class Root(string path) : Folder(string.Empty)
    {
        /// <summary>
        /// The path of the root folder.
        /// </summary>
        private readonly string _rootPath = path;

        /// <summary>
        /// Gets the path of the root folder.
        /// </summary>
        public override string Path => _rootPath;
    }
}