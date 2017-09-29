using System;
using System.Collections.Generic;
using System.IO;

namespace Crawler.Logic
{
    internal static class ItemWriter
    {
        public static void Write(List<Item> items, string outputPath)
        {
            foreach (var item in items)
            {
                var dirPath = Path.Combine(outputPath, new Uri(item.Path).Host);
                Directory.CreateDirectory(dirPath);
                Write(item, dirPath);
            }
        }

        /*смапить на структуру папок а не писать напрямую
         * иначе в момент когда нужно будет заменить все url в html я не смогу указать на правильную директорию
         * 
         * как альтернатива делать все в один проход
         * билдер оркестратор
         * сразу формируем путь в файловой системе, отдаем формирователю конфигурацию запуска, оттуда он возьмет выходную директорию
         * сразу формируем список рутов
        */
        private static void Write(Item item, string outputPath)
        {
            var uri = new Uri(item.Path);
            var fileName = Path.GetFileName(uri.LocalPath);
            var localPath =
                uri.LocalPath.Remove(uri.LocalPath.LastIndexOf(fileName, StringComparison.InvariantCultureIgnoreCase),
                    fileName.Length);
            if (string.IsNullOrEmpty(localPath) && item.ByteContent != null)
                throw new Exception("Unknown file content");
            var directoryPath = PathCombine(outputPath, localPath);
            var di = new DirectoryInfo(directoryPath);
            if(!di.Exists)
                di.Create();

            var outPath = $"{outputPath}{localPath}{GetFileNameOrDefault(fileName)}";
            var fileInfo = new FileInfo(outPath);
            if(fileInfo.Exists)
                fileInfo.Delete();

            if (item.ByteContent != null)
            {
                using (var stream = fileInfo.Create())
                {
                    stream.Write(item.ByteContent, 0, item.ByteContent.Length);
                    stream.Flush();
                }
            }
            else
            {
                using (var writer = fileInfo.CreateText())
                {
                    writer.Write(item.Content);
                    writer.Flush();
                }
            }

            foreach(var i in item.GetSubItems())
                Write(i, directoryPath);
        }

        public static List<Item> ExtractRoots(Item item, List<Item> rootItems)
        {
            foreach (var subItem in item.GetSubItems())
            {
                if (UrlHelper.IsExternalLink(subItem.Path))
                {
                    item.RemoveItem(subItem);
                    rootItems.Add(subItem);
                }
                ExtractRoots(subItem, rootItems);
            }

            return rootItems;
        }

        private static string GetFileNameOrDefault(string fileName)
        {
            return string.IsNullOrEmpty(fileName) ? "index.html" : fileName;
        }

        private static string PathCombine(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1))
                return path2;
            if (string.IsNullOrEmpty(path2))
                return path1;
            string separator = Path.DirectorySeparatorChar.ToString();
            string altSeparator = Path.AltDirectorySeparatorChar.ToString();
            if (path1.EndsWith(separator) || path1.EndsWith(altSeparator) ||
                path2.StartsWith(separator) || path2.StartsWith(altSeparator))
                return path1 + path2;

            return $"{path1}\\{path2}";
        }
    }
}
