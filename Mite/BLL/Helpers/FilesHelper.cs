using ImageMagick;
using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace Mite.BLL.Helpers
{
    public static class FilesHelper
    {
        /// <summary>
        /// Тип вложения по его mime типу
        /// </summary>
        public static AttachmentTypes AttachmentTypeMime(string contentType)
        {
            var fileType = contentType.Split('/').First();
            switch (fileType)
            {
                case "image":
                    return AttachmentTypes.Image;
                case "audio":
                    return AttachmentTypes.Audio;
                case "video":
                    return AttachmentTypes.Video;
                case "text":
                default:
                    return AttachmentTypes.Text;
            }
        }
        /// <summary>
        /// Сохраняет изображение в файловой системе
        /// </summary>
        /// <param name="virtualPath">Относительный путь к папке сохранения</param>
        /// <param name="base64Str">Само изображение(в кодировке base64)</param>
        /// <returns>Относительный путь к сохраненному изображению</returns>
        public static string CreateImage(string virtualPath, string base64Str)
        {
            var path = HostingEnvironment.MapPath(virtualPath);
            var imgFormat = Regex.Match(base64Str, @"data:image/(.+);base64,").Groups[1].Value;
            if (imgFormat == "jpeg")
                imgFormat = "jpg";
            base64Str = Regex.Replace(base64Str, @"data:image/(.+);base64,", "");
            //Кодируем строку base64 в изображение
            var bytes = Convert.FromBase64String(base64Str);
            using(var mStream = new MemoryStream(bytes))
            {
                var optimizer = new ImageOptimizer();
                optimizer.LosslessCompress(mStream);

                string imageName;
                string imagePath;
                //Проверяем на то, есть ли уже изображения с таким именем
                do
                {
                    imageName = Guid.NewGuid() + "." + imgFormat;
                    imagePath = path[path.Length - 1] == '\\' ? path + imageName : path + "\\" + imageName;
                } while (File.Exists(imagePath));
                //Сохраняем на диск
                using (var fStream = new FileStream(imagePath, FileMode.CreateNew, FileAccess.Write))
                {
                    mStream.Position = 0;
                    mStream.WriteTo(fStream);
                }

                virtualPath += virtualPath[virtualPath.Length - 1] == '/' ? "" : "/";
                return Regex.Replace(virtualPath, "~", "") + imageName;
            }
        }
        public static string CreateFile(string virtualPath, string base64, string ext)
        {
            if (string.IsNullOrEmpty(virtualPath))
                throw new ArgumentNullException("Virtual path is null");
            base64 = Regex.Replace(base64, @"^data:.+;base64,", string.Empty);
            string fullFolder = HostingEnvironment.MapPath(virtualPath),
                filePath;
            do
            {
                filePath = Path.Combine(fullFolder, $"{Guid.NewGuid()}.{ext}");
            } while (File.Exists(filePath));

            File.WriteAllBytes(filePath, Convert.FromBase64String(base64));
            return filePath.Replace(HostingEnvironment.ApplicationPhysicalPath, "/");
        }
        public static string CreateFile(string virtualFolder, HttpPostedFileBase file)
        {
            string fullFolder = HostingEnvironment.MapPath(virtualFolder), 
                ext = file.FileName.Split('.').Last(),
                filePath;
            do
            {
                filePath = Path.Combine(fullFolder, $"{Guid.NewGuid()}.{ext}");
            } while (File.Exists(filePath));

            file.SaveAs(filePath);
            return filePath.Replace(HostingEnvironment.ApplicationPhysicalPath, "/").Replace('\\', '/');
        }
        public static string CreateDocument(string virtualPath, string content, string ext = "dat")
        {
            var path = HostingEnvironment.MapPath(virtualPath);
            string filePath;
            string fileName;
            if(path == null)
                throw new NullReferenceException("Вы не задали путь");

            do
            {
                fileName = Guid.NewGuid() + $".{ext}";
                filePath = path[path.Length - 1] == '\\' ? path + fileName : path + "\\" + fileName;
            } while (File.Exists(filePath));

            File.WriteAllText(filePath, content, Encoding.UTF8);
            virtualPath += virtualPath[virtualPath.Length - 1] == '/' ? "" : "/";
            return Regex.Replace(virtualPath, "~", "") + fileName;
        }
        public static void UpdateDocument(string fileVirtualPath, string content)
        {
            if (fileVirtualPath == null)
                throw new NullReferenceException("Вы не задали путь к файлу");

            var path = HostingEnvironment.MapPath(fileVirtualPath);

            File.WriteAllText(path, content);
        }
        /// <summary>
        /// Удаляем файл по относительному пути
        /// </summary>
        /// <param name="virtualPath"></param>
        public static void DeleteFile(string virtualPath)
        {
            if (!string.IsNullOrEmpty(virtualPath))
            {
                var path = HostingEnvironment.MapPath(virtualPath);
                DeleteFileFull(path);
            }
        }
        /// <summary>
        /// Удаляем файл по полному пути
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFileFull(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                File.Delete(path);
        }
        /// <summary>
        /// Проверяет, путь ли это
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool IsPath(string val)
        {
            if(Regex.IsMatch(val, "^\\/") || Regex.IsMatch(val, "^~\\/"))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Считывает документ
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<string> ReadDocumentAsync(string path)
        {
            if (!File.Exists(HostingEnvironment.MapPath(path)))
                return "";
            var reader = new StreamReader(HostingEnvironment.MapPath(path));
            var result = await reader.ReadToEndAsync();
            reader.Close();
            return result;
        }
        /// <summary>
        /// Кол-во символов в документе
        /// </summary>
        /// <param name="path"></param>
        public static long GetDocCharsCount(string path)
        {
            var fileInfo = new FileInfo(HostingEnvironment.MapPath(path));
            return fileInfo.Length;
        }
        /// <summary>
        /// Считывает определенное кол-во символов в документе(для списка постов)
        /// </summary>
        /// <param name="path">путь</param>
        /// <param name="charsCount">кол-во символов</param>
        /// <returns></returns>
        public static async Task<string> ReadDocumentAsync(string path, int charsCount)
        {
            if (!File.Exists(HostingEnvironment.MapPath(path)))
                return "";

            var reader = new StreamReader(HostingEnvironment.MapPath(path));
            var str = await reader.ReadToEndAsync();
            reader.Close();
            return HandleReading(str, path, charsCount);
        }
        public static string ReadDocument(string path, int charsCount)
        {
            if (!File.Exists(HostingEnvironment.MapPath(path)))
                return "";

            var reader = new StreamReader(HostingEnvironment.MapPath(path));
            var str = reader.ReadToEnd();
            reader.Close();
            return HandleReading(str, path, charsCount);
        }
        private static string HandleReading(string str, string path, int charsCount)
        {
            //После удаляем картинки, т.к. они будут некорректно отображаться
            str = Regex.Replace(str, @"<img.+(>|\/>)", "");
            str = Regex.Replace(str, @"<table>.+</table>", "");

            var substr = str.Substring(0, str.Length < charsCount ? str.Length : charsCount);

            //Удаляем все недооткрытые(вроде <p) в конце теги
            substr = Regex.Replace(substr, @"(<|<\/|<(h3|h2|p|i|b)[^>]*>*)$", "");

            substr = Regex.Replace(substr, @"(<h3[^>]*|h2[^>]*)", "<p");

            var tags = new[] { "p", "i", "b" };
            var closeTagsStack = new Stack<string>();
            if (charsCount < GetDocCharsCount(path))
            {
                substr += "...";
            }

            foreach (var tag in tags)
            {
                substr = Regex.Replace(substr, $"<\\/{tag}[ ]*$", $"<\\/{tag}>");
                //Проверяем, закрыт ли тег в конце, т.к. мы обрезали строку
                var lastTagMatches = Regex.Matches(substr, $"<{tag}[^>]*>");
                if (lastTagMatches.Count == 0)
                    continue;

                var lastTag = lastTagMatches[lastTagMatches.Count - 1];
                var lastTagCloseMatches = Regex.Matches(substr, $"</{tag}");
                if (lastTagCloseMatches.Count > 0)
                {
                    var lastTagClose = lastTagCloseMatches[lastTagCloseMatches.Count - 1];
                    if (lastTag.Index > lastTagClose.Index)
                    {
                        closeTagsStack.Push(tag);
                    }
                }
                else
                {
                    closeTagsStack.Push(tag);
                }
            }
            foreach (var closeTag in closeTagsStack)
            {
                substr += $"</{closeTag}>";
            }
            return substr;
        }
    }
}