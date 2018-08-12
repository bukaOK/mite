using ImageMagick;
using Mite.BLL.Core;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace Mite.BLL.Helpers
{
    public static class ImagesHelper
    {
        public static string WatermarkFontPath => HostingEnvironment.MapPath("/Files/roboto.ttf");

        private const string CompressedPostfix = "compressed";
        private static readonly string ImagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";

        public static byte[] DrawWatermark(int fontSize, string text, bool invert, string fontFamily = null)
        {
            if (fontFamily == null)
                fontFamily = WatermarkFontPath;
            text = text.ToUpper();
            var color = invert ? Color.FromArgb(128, Color.White) : Color.FromArgb(128, Color.Black);
            const int kerning = 6;
            using (var measure = new MagickImage(Color.Transparent, 1, 1))
            {
                measure.Settings.FontFamily = fontFamily;
                measure.Settings.FontWeight = FontWeight.Light;
                measure.Settings.FontPointsize = fontSize;
                measure.Settings.TextKerning = kerning;
                var metric = measure.FontTypeMetrics(text);
                using (var img = new MagickImage(Color.Transparent, (int)metric.TextWidth, (int)metric.TextHeight + 5))
                {
                    new Drawables()
                        .Font(fontFamily)
                        .StrokeColor(color)
                        .TextKerning(kerning)
                        .FillColor(color)
                        .TextAlignment(TextAlignment.Center)
                        .FontPointSize(fontSize)
                        .Text(metric.TextWidth / 2, fontSize, text)
                        .Draw(img);

                    return img.ToByteArray(MagickFormat.Png);
                }
            }
        }
        public static bool IsAnimatedImage(string path)
        {
            if(Path.GetExtension(path) == ".gif")
            {
                var info = MagickFormatInfo.Create(path);
                return info.IsMultiFrame;
            }
            return false;
        }
        /// <summary>
        /// Обновляем изображение
        /// </summary>
        /// <param name="oldVPath">Путь(вирт.) к старому оригинальному изображению</param>
        /// <param name="base64">Строка изображения с base64</param>
        /// <returns>vPath - вирт. путь к изобр., compressedVPath - вирт. путь к сжатому изобр.</returns>
        public static string UpdateImage(string oldVPath, string folder, string base64)
        {
            string vPath = null;
            try
            {
                vPath = FilesHelper.CreateImage(oldVPath, base64);
                var fullPath = HostingEnvironment.MapPath(vPath);
                //Удаляем старые
                FilesHelper.DeleteFile(oldVPath);
                return vPath;
            }
            catch(Exception e)
            {
                if (vPath != null)
                    FilesHelper.DeleteFile(vPath);
                throw e;
            }
        }
        /// <summary>
        /// Обновляем изображение
        /// </summary>
        /// <param name="oldVPath">Путь(вирт.) к старому оригинальному изображению</param>
        /// <param name="file">Поток с изображением</param>
        /// <returns>vPath - вирт. путь к изобр., compressedVPath - вирт. путь к сжатому изобр.</returns>
        public static string UpdateImage(string oldVPath, HttpPostedFileBase file)
        {
            string vPath = null;
            try
            {
                vPath = FilesHelper.CreateImage(ImagesFolder, file);
                var fullPath = HostingEnvironment.MapPath(vPath);
                //Удаляем старые
                FilesHelper.DeleteFile(oldVPath);
                return vPath;
            }
            catch (Exception e)
            {
                if (vPath != null)
                    FilesHelper.DeleteFile(vPath);
                throw e;
            }
        }
        /// <summary>
        /// Создает изображение с заданной шириной и высотой
        /// </summary>
        /// <param name="base64">Изображение в base64</param>
        /// <param name="saveFolder">Папка для сохранения</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="adaptive"></param>
        /// <returns>Относительный путь к созданному изображению</returns>
        public static string Create(string saveFolder, string base64, int width, int? height = null)
        {
            var imgFormat = Regex.Match(base64, @"data:image/(.+);base64,").Groups[1].Value;
            if (imgFormat == "jpeg")
                imgFormat = "jpg";
            base64 = Regex.Replace(base64, @"data:image/(.+);base64,", "");
            var savePath = "";
            do
            {
                savePath = Path.Combine(saveFolder, $"{Guid.NewGuid()}.{imgFormat}");
            } while (File.Exists(savePath));

            var fullPath = HostingEnvironment.MapPath(savePath);
            using(var img = new MagickImage(Convert.FromBase64String(base64)))
            {
                if (height == null)
                    height = width * img.Height / img.Width;
                var size = new MagickGeometry(width, (int)height);
                
                img.Resize(size);
                img.Write(fullPath);
            }
            return savePath;
        }
        public static string Create(string saveFolder, HttpPostedFileBase img, int width, int? height = null, bool adaptive = true)
        {
            if (img.ContentType.Split('/')[0] != "image")
                return null;
            var imgFormat = img.ContentType.Split('/')[1];
            if (imgFormat == "jpeg")
                imgFormat = "jpg";
            var savePath = "";
            do
            {
                savePath = Path.Combine(saveFolder, $"{Guid.NewGuid()}.{imgFormat}");
            } while (File.Exists(savePath));

            var fullPath = HostingEnvironment.MapPath(savePath);
            using (var mStream = new MemoryStream())
            {
                img.InputStream.CopyTo(mStream);
                mStream.Position = 0;
                using(var magick = new MagickImage(mStream))
                {
                    magick.ColorAlpha(new MagickColor(Color.White));
                    if (height == null)
                        height = width * magick.Height / magick.Width;
                    var size = new MagickGeometry(width, (int)height);
                    if (adaptive)
                        magick.AdaptiveResize(size);
                    else
                        magick.Resize(size);
                    magick.Write(fullPath);
                }
            }
            return savePath;
        }
        /// <summary>
        /// Изменить размер изображения
        /// </summary>
        /// <param name="path">Полный путь к изображению</param>
        /// <param name="width">Ширина изображения</param>
        /// <param name="height">Высота(если null - с сохранением пропорций)</param>
        /// <param name="adaptive">Ч/з билинейную интерполяцию. Подходит, если нужно небольшое изображение(высокая скорость)</param>
        /// <returns>Полный путь к измененному изображению</returns>
        public static string Resize(string path, int width, int? height = null, bool adaptive = true)
        {
            var match = Regex.Match(path, @"(?<path>.+)\\(?<fileName>[\w\d-]+)\.(?<ext>[\w\d]+)", RegexOptions.Compiled);
            var savePath = Path.Combine(match.Groups["path"].Value, $"{match.Groups["fileName"].Value}_{width}.{match.Groups["ext"].Value}");

            using (var img = new MagickImage(path))
            {
                if (height == null)
                    height = width * img.Height / img.Width;
                var size = new MagickGeometry(width, (int)height);
                if (adaptive)
                    img.AdaptiveResize(size);
                else
                    img.Resize(size);
                img.Write(savePath);
            }
            return savePath;
        }
        public static class Compressed
        {
            public static string CompressedVirtualPath(string path)
            {
                var compressedFullPath = CompressedPath(path);
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
                var format = customFormat ?? "jpg";
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