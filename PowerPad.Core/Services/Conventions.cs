namespace PowerPad.Core.Services
{
    public static class Conventions
    {
        public const string AUTO_SAVE_EXTENSION = ".autosave";
        public static string AutosavePath(string path)
        {
            return $"{path}{AUTO_SAVE_EXTENSION}";
        }

        public static readonly string CONFIG_FOLDER_NAME = $".{nameof(PowerPad).ToLower()}";
        public const string TRASH_FOLDER_NAME = ".trash";
        public const string ORDER_FILE_NAME = ".order";
    }
}