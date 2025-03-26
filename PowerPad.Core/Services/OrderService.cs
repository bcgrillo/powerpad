using PowerPad.Core.Contracts;
using PowerPad.Core.Models;
using System.Text.Json;
using static PowerPad.Core.Services.Conventions;
using static PowerPad.Core.Configuration.ConfigConstants;

namespace PowerPad.Core.Services
{
    public interface IOrderService
    {
        void LoadOrderRecursive(Folder root);
        void UpdateOrderAfterCreation(Folder parentFolder, string newEntryName);
        void UpdateOrderAfterDeletion(Folder parentFolder, string deletedEntryName);
        void UpdateOrderAfterRename(Folder parentFolder, string entryNewName, string entryOldName);
        void UpdateOrderAfterMove(Folder sourceFolder, Folder? targetFolder, string movedEntryName, int newPosition);
    }

    public class OrderService : IOrderService
    {
        public void LoadOrderRecursive(Folder folder)
        {
            LoadOrder(folder);

            if (folder.Folders != null)
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

        public void UpdateOrderAfterRename(Folder parentFolder, string entryNewName, string entryOldName)
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

            if (targetFolder != null)
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

        private static IList<string> LoadOrder(Folder parentFolder)
        {
            var order = parentFolder.Order;

            //Initialize order
            if (order == null)
            {
                var orderAux = new List<string>();

                if (parentFolder.Folders != null) foreach (var folder in parentFolder.Folders) orderAux.Add(folder.Name);

                if (parentFolder.Documents != null) foreach (var document in parentFolder.Documents) orderAux.Add($"{document.Name}{document.Extension}");

                var orderFilePath = Path.Combine(parentFolder.Path, ORDER_FILE_NAME);

                if (File.Exists(orderFilePath))
                {
                    order = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(orderFilePath)) ?? orderAux;

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

        private static void SaveOrder(Folder parentFolder, IList<string> order)
        {
            var orderFilePath = Path.Combine(parentFolder.Path, ORDER_FILE_NAME);

            File.WriteAllText(orderFilePath, JsonSerializer.Serialize(order, JSON_SERIALIZER_OPTIONS));

            parentFolder.Order = order;
        }
    }
}
