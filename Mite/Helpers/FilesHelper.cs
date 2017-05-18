﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace Mite.Helpers
{
    public static class FilesHelper
    {
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
            var memoryStream = new MemoryStream(bytes);

            var image = Image.FromStream(memoryStream);

            string imageName;
            string imagePath;
            //Проверяем на то, есть ли уже изображения с таким именем
            do
            {
                imageName = Guid.NewGuid() + "." + imgFormat;
                imagePath = path[path.Length - 1] == '\\' ? path + imageName : path + "\\" + imageName;
            } while (File.Exists(imagePath));
            //Сохраняем на диск
            image.Save(imagePath);
            memoryStream.Close();

            virtualPath += virtualPath[virtualPath.Length - 1] == '/' ? "" : "/";
            return Regex.Replace(virtualPath, "~", "") + imageName;
        }
        public static string CreateImage(string virtualPath, HttpPostedFileBase image)
        {
            var path = HostingEnvironment.MapPath(virtualPath);
            var imageNameArr = image.FileName.Split('.');
            var imgFormat = imageNameArr[imageNameArr.Length - 1];
            if (imgFormat != "gif" && imgFormat != "jpg" && imgFormat != "png")
                return null;

            string imageName;
            string imagePath;
            //Проверяем на то, есть ли уже изображения с таким именем
            do
            {
                imageName = Guid.NewGuid() + "." + imgFormat;
                imagePath = path[path.Length - 1] == '\\' ? path + imageName : path + "\\" + imageName;
            } while (File.Exists(imagePath));

            image.SaveAs(imagePath);
            virtualPath += virtualPath[virtualPath.Length - 1] == '/' ? "" : "/";
            return Regex.Replace(virtualPath, "~", "") + imageName;
        }
        public static string CreateDocument(string virtualPath, string content)
        {
            var path = HostingEnvironment.MapPath(virtualPath);
            string filePath;
            string fileName;
            if(path == null)
                throw new NullReferenceException("Вы не задали путь");

            do
            {
                fileName = Guid.NewGuid() + ".dat";
                filePath = path[path.Length - 1] == '\\' ? path + fileName : path + "\\" + fileName;
            } while (File.Exists(filePath));

            File.WriteAllText(filePath, content, Encoding.UTF8);
            virtualPath += virtualPath[virtualPath.Length - 1] == '/' ? "" : "/";
            return Regex.Replace(virtualPath, "~", "") + fileName;
        }

        public static void UpdateDocument(string filePath, string content)
        {
            filePath = HostingEnvironment.MapPath(filePath);

            if (filePath == null)
                throw new NullReferenceException("Вы не задали путь к файлу");

            File.WriteAllText(filePath, content);
        }

        public static void DeleteFile(string virtualFilePath)
        {
            var filePath = HostingEnvironment.MapPath(virtualFilePath);
            if(File.Exists(filePath))
                File.Delete(filePath);
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
            var reader = new StreamReader(HostingEnvironment.MapPath(path));
            var str = await reader.ReadToEndAsync();
            reader.Close();
            //После удаляем картинки, т.к. они будут некорректно отображаться
            str = Regex.Replace(str, @"<img.+(>|\/>)", "");
            str = Regex.Replace(str, @"<table>.+</table>", "");

            var substr = str.Substring(0, str.Length < charsCount ? str.Length : charsCount);
            if (charsCount < GetDocCharsCount(path))
            {
                substr += "...";
            }
            //Проверяем, закрыт ли тег в конце, т.к. мы обрезали строку
            var lastPMatches = Regex.Matches(substr, "<p[^>]*>");
            var lastP = lastPMatches[lastPMatches.Count - 1];
            var lastPCloseMatches = Regex.Matches(substr, "</p>");
            if (lastPCloseMatches.Count > 0)
            {
                var lastPClose = lastPCloseMatches[lastPCloseMatches.Count - 1];
                if (lastP.Index > lastPClose.Index)
                    substr += "</p>";
            }
            else
            {
                substr += "<p/>";
            }

            return substr;
        }
        public static string ReadDocument(string path, int charsCount)
        {
            var reader = new StreamReader(HostingEnvironment.MapPath(path));
            var buffer = new char[charsCount];
            reader.Read(buffer, 0, charsCount);
            reader.Close();
            return new string(buffer);
        }
        //public static Image ResizeImage(Image sourceImg, int width, int height = 0)
        //{
        //}
    }
}