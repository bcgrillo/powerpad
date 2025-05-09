namespace PowerPad.Core.Services
{
    /// <summary>
    /// Provides utility methods and constants for handling application conventions.
    /// </summary>
    public static class Conventions
    {
        /// <summary>
        /// The file extension used for auto-saved files.
        /// </summary>
        public const string AUTO_SAVE_EXTENSION = ".autosave";

        /// <summary>
        /// Generates the auto-save file path by appending the auto-save extension to the given path.
        /// </summary>
        /// <param name="path">The base file path.</param>
        /// <returns>The auto-save file path.</returns>
        public static string AutosavePath(string path)
        {
            return $"{path}{AUTO_SAVE_EXTENSION}";
        }

        /// <summary>
        /// The name of the folder used to store deleted files.
        /// </summary>
        public const string TRASH_FOLDER_NAME = ".trash";

        /// <summary>
        /// The name of the file used to store order information.
        /// </summary>
        public const string ORDER_FILE_NAME = ".order";
    }
}