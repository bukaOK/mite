using ImageMagick;
using Mite.BLL.Core;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Mite.BLL.Helpers
{
    public static class ImagesHelper
    {
        private const string CompressedPostfix = "compressed";
        private static readonly string ImagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";

        /// <summary>
        /// Получаем расширение оригинального файла
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns></returns>
        public static string GetFormat(string path)
        {
            var pathArr = path.Split('.');
            return pathArr[pathArr.Length - 1];
        }
        public static bool IsAnimatedImage(string path)
        {
            if(path.Split('.').Last() == "gif")
            {
                using (var img = Image.FromFile(path))
                {
                    return img.GetFrameCount(new FrameDimension(img.FrameDimensionsList[0])) > 1;
                }
            }
            return false;
        }
        /// <summary>
        /// Обновляем изображение
        /// </summary>
        /// <param name="oldVPath">Путь(вирт.) к старому оригинальному изображению</param>
        /// <param name="oldCompressedVPath">Путь(вирт.) к старому сжатому изображению</param>
        /// <param name="base64">Строка изображения с base64</param>
        /// <returns>vPath - вирт. путь к изобр., compressedVPath - вирт. путь к сжатому изобр.</returns>
        public static (string vPath, string compressedVPath) UpdateImage(string oldVPath, string oldCompressedVPath, string base64)
        {
            string vPath = null, compressedVPath = null;
            try
            {
                vPath = FilesHelper.CreateImage(ImagesFolder, base64);
                var fullPath = HostingEnvironment.MapPath(vPath);
                Compressed.Compress(fullPath);
                compressedVPath = Compressed.CompressedVirtualPath(fullPath);
                //Удаляем старые
                FilesHelper.DeleteFile(oldVPath);
                FilesHelper.DeleteFile(oldCompressedVPath);
                return (vPath, compressedVPath);
            }
            catch(Exception e)
            {
                DeleteImage(vPath, compressedVPath);
                throw e;
            }
        }
        public static (string vPath, string compressedVPath) CreateImage(string imagesFolder, string base64)
        {
            string vPath = null, compressedVPath = null;
            try
            {
                vPath = FilesHelper.CreateImage(imagesFolder, base64);
                var fullPath = HostingEnvironment.MapPath(vPath);
                Compressed.Compress(fullPath);
                compressedVPath = Compressed.CompressedVirtualPath(fullPath);

                return (vPath, compressedVPath);
            }
            catch(Exception e)
            {
                DeleteImage(vPath, compressedVPath);
                throw e;
            }
        }
        public static (string vPath, string compressedVPath) CreateImage(string imagesFolder, HttpPostedFileBase image)
        {
            string vPath = null, compressedVPath = null;
            try
            {
                vPath = FilesHelper.CreateFile(imagesFolder, image);
                var fullPath = HostingEnvironment.MapPath(vPath);
                Compressed.Compress(fullPath);
                compressedVPath = Compressed.CompressedVirtualPath(fullPath);

                return (vPath, compressedVPath);
            }
            catch(Exception e)
            {
                DeleteImage(vPath, compressedVPath);
                throw e;
            }
        }
        public static void DeleteImage(string vPath, string compressedVPath)
        {
            var fullImgPath = HostingEnvironment.MapPath(vPath);
            FilesHelper.DeleteFile(vPath);
            if (string.IsNullOrEmpty(compressedVPath))
                FilesHelper.DeleteFile(Compressed.CompressedVirtualPath(fullImgPath));
            else
                FilesHelper.DeleteFile(compressedVPath);
        }
        public static class Compressed
        {
            public static string CompressedVirtualPath(string path)
            {
                var compressedFullPath = CompressedPath(path, "jpg");
                var virtualPath = compressedFullPath.Replace(HostingEnvironment.ApplicationPhysicalPath, string.Empty);
                if (virtualPath[0] != '\\' && virtualPath[0] != '/')
                    virtualPath = "\\" + virtualPath;
                return virtualPath.Replace('\\', '/');
            }
            /// <summary>
            /// Генерируем путь к сжатому изображению
            /// </summary>
            /// <param name="path">Путь к оригинальному изображению</param>
            /// <param name="customFormat">Пользовательское расширение файла</param>
            /// <returns></returns>
            public static string CompressedPath(string path, string customFormat = null)
            {
                if(path[path.Length - 1] == '\\')
                {
                    path = path.Substring(0, path.Length - 1);
                }
                //Разделяем путь к файлу, чтобы получить его имя
                var pathArr = path.Split('\\');
                var fileName = pathArr[pathArr.Length - 1].Split('.')[0];
                var folder = path.Replace(pathArr[pathArr.Length - 1], "");

                //Основываясь на названии самого изображения, создаем название сжатого изображения
                var format = customFormat ?? GetFormat(path);
                var compressedName = $"{fileName}_{CompressedPostfix}.{format}";

                return folder + compressedName;
            }
            /// <summary>
            /// Существует ли сжатый аналог изображения
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static bool CompressedExists(string path)
            {
                var compressedPath = CompressedPath(path, "jpg");
                return File.Exists(compressedPath);
            }

            private static ImageCodecInfo GetEncoder(ImageFormat format)
            {
                var codecs = ImageCodecInfo.GetImageDecoders();

                foreach (var codec in codecs)
                {
                    if (codec.FormatID == format.Guid)
                    {
                        return codec;
                    }
                }
                return null;
            }
            /// <summary>
            /// Сжимает изображение
            /// </summary>
            /// <param name="path">Полный путь к оригинальному изображению</param>
            /// <returns></returns>
            public static DataServiceResult Compress(string path)
            {
                try
                {
                    using (var image = new MagickImage(path))
                    {
                        var compressedPath = CompressedPath(path, "jpg");
                        image.ColorAlpha(MagickColors.White);
                        image.Format = MagickFormat.Jpeg;
                        image.Quality = 50;
                        image.Write(compressedPath);
                        return DataServiceResult.Success(compressedPath);
                    }
                }
                catch(MagickBlobErrorException)
                {
                    return DataServiceResult.Failed("Неизвестный формат");
                }
                catch (Exception)
                {
                    return DataServiceResult.Failed("Ошибка при сжатии");
                }
            }
        }
    }
}