using PowerPad.Core.Models.FileSystem;
using System.Text.Json;
using System.Text.Json.Serialization;
using static PowerPad.Core.Services.Conventions;

namespace PowerPad.Core.Services.FileSystem
{
    /// <summary>
    /// Provides methods to manage and update the order of entries (folders and documents) within a folder.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Loads the order of entries recursively for the specified root folder and its subfolders.
        /// </summary>
        /// <param name="root">The root folder to load the order for.</param>
        void LoadOrderRecursive(Folder root);

        /// <summary>
        /// Updates the order of entries after a new entry is created in the specified parent folder.
        /// </summary>
        /// <param name="parentFolder">The parent folder where the new entry is created.</param>
        /// <param name="newEntryName">The name of the newly created entry.</param>
        void UpdateOrderAfterCreation(Folder parentFolder, string newEntryName);

        /// <summary>
        /// Updates the order of entries after an entry is deleted from the specified parent folder.
        /// </summary>
        /// <param name="parentFolder">The parent folder where the entry is deleted.</param>
        /// <param name="deletedEntryName">The name of the deleted entry.</param>
        void UpdateOrderAfterDeletion(Folder parentFolder, string deletedEntryName);

        /// <summary>
        /// Updates the order of entries after an entry is renamed in the specified parent folder.
        /// </summary>
        /// <param name="parentFolder">The parent folder where the entry is renamed.</param>
        /// <param name="entryOldName">The old name of the entry.</param>
        /// <param name="entryNewName">The new name of the entry.</param>
        void UpdateOrderAfterRename(Folder parentFolder, string entryOldName, string entryNewName);

        /// <summary>
        /// Updates the order of entries after an entry is moved between folders or within the same folder.
        /// </summary>
        /// <param name="sourceFolder">The source folder where the entry is moved from.</param>
        /// <param name="targetFolder">The target folder where the entry is moved to. Can be null if moving within the same folder.</param>
        /// <param name="movedEntryName">The name of the moved entry.</param>
        /// <param name="newPosition">The new position of the entry in the target folder.</param>
        void UpdateOrderAfterMove(Folder sourceFolder, Folder? targetFolder, string movedEntryName, int newPosition);
    }

    /// <summary>
    /// Implementation of <see cref="IOrderService"/> that manages the order of entries within folders.
    /// </summary>
    public class OrderService(JsonSerializerContext context) : IOrderService
    {
        private readonly JsonSerializerContext _context = context;

        /// <inheritdoc />
        public void LoadOrderRecursive(Folder folder)
        {
            LoadOrder(folder);

            if (folder.Folders is not null)
            {
                foreach (var subFolder in folder.Folders) LoadOrderRecursive(subFolder);
            }
        }

        /// <inheritdoc />
        public void UpdateOrderAfterCreation(Folder parentFolder, string newEntryName)
        {
            var orderedEntries = LoadOrder(parentFolder);

            orderedEntries.Remove(newEntryName);

            orderedEntries.Add(newEntryName);

            SaveOrder(parentFolder, orderedEntries);
        }

        /// <inheritdoc />
        public void UpdateOrderAfterDeletion(Folder parentFolder, string deletedEntryName)
        {
            var orderedEntries = LoadOrder(parentFolder);

            orderedEntries.Remove(deletedEntryName);

            SaveOrder(parentFolder, orderedEntries);
        }

        /// <inheritdoc />
        public void UpdateOrderAfterRename(Folder parentFolder, string entryOldName, string entryNewName)
        {
            var orderedEntries = LoadOrder(parentFolder);

            var index = orderedEntries.IndexOf(entryOldName);

            if (index == -1)
            {
                orderedEntries.Add(entryNewName);
            }
            else
            {
                orderedEntries[index] = entryNewName;
            }

            SaveOrder(parentFolder, orderedEntries);
        }

        /// <inheritdoc />
        public void UpdateOrderAfterMove(Folder sourceFolder, Folder? targetFolder, string movedEntryName, int newPosition)
        {
            var sourceOrderedEntries = LoadOrder(sourceFolder);

            sourceOrderedEntries.Remove(movedEntryName);

            if (targetFolder is not null)
            {
                var targetOrderedEntries = LoadOrder(targetFolder);

                targetOrderedEntries.Remove(movedEntryName);

                newPosition = Math.Clamp(newPosition, 0, targetOrderedEntries.Count);
                targetOrderedEntries.Insert(newPosition, movedEntryName);

                SaveOrder(targetFolder, targetOrderedEntries);
            }
            else
            {
                newPosition = Math.Clamp(newPosition, 0, sourceOrderedEntries.Count);
                sourceOrderedEntries.Insert(newPosition, movedEntryName);
            }

            SaveOrder(sourceFolder, sourceOrderedEntries);
        }

        /// <summary>
        /// Loads the order of entries for the specified folder. Initializes the order if it does not exist.
        /// </summary>
        /// <param name="parentFolder">The folder to load the order for.</param>
        /// <returns>A list of ordered entry names.</returns>
        private List<string> LoadOrder(Folder parentFolder)
        {
            var order = parentFolder.Order;

            // Initialize order
            if (order is null)
            {
                var orderAux = new List<string>();

                if (parentFolder.Folders is not null) foreach (var folder in parentFolder.Folders) orderAux.Add(folder.Name);

                if (parentFolder.Documents is not null) foreach (var document in parentFolder.Documents) orderAux.Add($"{document.Name}{document.Extension}");

                var orderFilePath = Path.Combine(parentFolder.Path, ORDER_FILE_NAME);

                if (File.Exists(orderFilePath))
                {
                    order = (List<string>?)JsonSerializer.Deserialize(File.ReadAllText(orderFilePath), typeof(List<string>), _context) ?? orderAux;

                    var elementsToRemove = order.Except(orderAux);
                    if (elementsToRemove.Any())
                    {
                        order = [.. order.Where(element => !elementsToRemove.Contains(element))];

                        SaveOrder(parentFolder, order);
                    }
                }
                else
                {
                    order = orderAux;
                }
            }

            parentFolder.Order = order;

            return order;
        }

        /// <summary>
        /// Saves the order of entries for the specified folder to a file.
        /// </summary>
        /// <param name="parentFolder">The folder to save the order for.</param>
        /// <param name="order">The list of ordered entry names to save.</param>
        private void SaveOrder(Folder parentFolder, List<string> order)
        {
            var orderFilePath = Path.Combine(parentFolder.Path, ORDER_FILE_NAME);

            File.WriteAllText(orderFilePath, JsonSerializer.Serialize(order, typeof(List<string>), _context));

            parentFolder.Order = order;
        }
    }
}