namespace PowerPad.Core.Models.FileSystem
{
    public class Root(string path) : Folder(string.Empty)
    {
        private readonly string _rootPath = path;

        public override string Path => _rootPath;
    }
}