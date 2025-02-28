//using PowerPad.Core.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PowerPad.WinUI.Helpers
//{
//    class DocumentTypeResolver
//    {
//        private EntryType GetFileType(string filePath)
//        {
//            string extension = Path.GetExtension(filePath).ToLower();
//            return extension switch
//            {
//                ".txt" => EntryType.Text,
//                ".md" => EntryType.Markdown,
//                ".todo" => EntryType.TodoList,
//                ".chat" => EntryType.Chat,
//                ".cs" => EntryType.Code,
//                ".js" => EntryType.Code,
//                ".html" => EntryType.Code,
//                ".css" => EntryType.Code,
//                ".java" => EntryType.Code,
//                ".py" => EntryType.Code,
//                ".cpp" => EntryType.Code,
//                ".c" => EntryType.Code,
//                ".h" => EntryType.Code,
//                ".htm" => EntryType.Code,
//                _ => EntryType.Unknown
//            };
//        }
//    }
//}
