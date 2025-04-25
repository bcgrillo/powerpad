using System.Text.Json;
using static PowerPad.Core.Services.Conventions;
using PowerPad.Core.Models.FileSystem;
using System.Text.Json.Serialization;

namespace PowerPad.Core.Services.FileSystem
{
    public interface IOrderService
    {
        void LoadOrderRecursive(Folder root);
        void UpdateOrderAfterCreation(Folder parentFolder, string newEntryName);
        void UpdateOrderAfterDeletion(Folder parentFolder, string deletedEntryName);
        void UpdateOrderAfterRename(Folder parentFolder, string entryOldName, string entryNewName);
        void UpdateOrderAfterMove(Folder sourceFolder, Folder? targetFolder, string movedEntryName, int newPosition);
    }

    public class OrderService(JsonSerializerContext context) : IOrderService
    {
        private readonly JsonSerializerContext _context = context;

        public void LoadOrderRecursive(Folder folder)
        {
            LoadOrder(folder);

            if (folder.Folders is not null)
            {
                foreach (var subFolder in folder.Folders) LoadOrderRecursive(subFolder);
            }
        }

        public void UpdateOrderAfterCreation(Folder parentFolder, string newEntryName)
        {
            var orderedEntries = LoadOrder(parentFolder);

            orderedEntries.Remove(newEntryName);

            orderedEntries.Add(newEntryName);

            SaveOrder(parentFolder, orderedEntries);
        }


        public void UpdateOrderAfterDeletion(Folder parentFolder, string deletedEntryName)
        {
            var orderedEntries = LoadOrder(parentFolder);

            orderedEntries.Remove(deletedEntryName);

            SaveOrder(parentFolder, orderedEntries);
        }

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

        private List<string> LoadOrder(Folder parentFolder)
        {
            var order = parentFolder.Order;

            //Initialize order
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

        private void SaveOrder(Folder parentFolder, List<string> order)
        {
            var orderFilePath = Path.Combine(parentFolder.Path, ORDER_FILE_NAME);

            File.WriteAllText(orderFilePath, JsonSerializer.Serialize(order, typeof(List<string>), _context));

            parentFolder.Order = order;
        }
    }
}
