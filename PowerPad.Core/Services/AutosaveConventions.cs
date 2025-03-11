namespace PowerPad.Core.Services
{
    public static class AutosaveConventions
    {
        public const string AUTO_SAVE_EXTENSION = ".autosave";
        public static string AutosavePath(string path)
        {
            return $"{path}{AUTO_SAVE_EXTENSION}";
        }
    }
}