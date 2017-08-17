﻿using Mite.BLL.Core;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
        public static class Compressed
        {
            public static string CompressedVirtualPath(string path)
            {
                var compressedFullPath = CompressedPath(path, "jpg");
                var virtualPath = compressedFullPath.Replace(HostingEnvironment.ApplicationPhysicalPath, string.Empty);
                if (virtualPath[0] != '\\' || virtualPath[0] != '/')
                    virtualPath = "\\" + virtualPath;                
                return virtualPath;
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
            public static DataServiceResult Compress(string path)
            {
                var compressResult = DataServiceResult.Failed();

                using (var originalImg = Image.FromFile(path))
                {
                    if (originalImg.RawFormat.Guid != ImageFormat.Jpeg.Guid && originalImg.RawFormat.Guid != ImageFormat.Png.Guid
                        && originalImg.RawFormat.Guid != ImageFormat.Gif.Guid)
                    {
                        return DataServiceResult.Failed("Изображение не подходит по формату");
                    }

                    var compressedPath = CompressedPath(path, "jpg");

                    if (originalImg.RawFormat.Guid == ImageFormat.Png.Guid)
                    {
                        compressResult = CompressPng(compressedPath, originalImg);
                    }
                    else if (originalImg.RawFormat.Guid == ImageFormat.Jpeg.Guid)
                    {
                        return CompressJpg(compressedPath, originalImg);
                    }
                    else if (originalImg.RawFormat.Guid == ImageFormat.Gif.Guid)
                    {
                        return CompressGif(compressedPath, originalImg);
                    }
                }
                return compressResult;
            }
            /// <summary>
            /// Сжимаем в Png
            /// </summary>
            /// <returns></returns>
            private static DataServiceResult CompressPng(string compressedPath, Image img)
            {
                using(var bmp = new Bitmap(img.Width, img.Height))
                {
                    bmp.SetResolution(img.HorizontalResolution, img.VerticalResolution);
                    using(var gr = Graphics.FromImage(bmp))
                    {
                        gr.Clear(Color.White);
                        gr.DrawImageUnscaled(img, 0, 0);
                    }
                    return CompressJpg(compressedPath, bmp);
                }                
            }
            /// <summary>
            /// Сжимаем в Jpeg
            /// </summary>
            /// <returns></returns>
            private static DataServiceResult CompressJpg(string compressedPath, Image img)
            {
                try
                {
                    var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L);
                    img.Save(compressedPath, jpgEncoder, encoderParams);
                    return DataServiceResult.Success();
                }
                catch (Exception e)
                {
                    return DataServiceResult.Failed($"Не удалось сжать изображение {e.Message}");
                }
            }
            /// <summary>
            /// Сохраняет один кадр из gifa
            /// </summary>
            /// <returns></returns>
            private static DataServiceResult CompressGif(string compressedPath, Image img)
            {
                try
                {
                    var frameDimension = new FrameDimension(img.FrameDimensionsList[0]);
                    img.SelectActiveFrame(frameDimension, 0);
                    img.Save(compressedPath, ImageFormat.Jpeg);
                    return DataServiceResult.Success();
                }
                catch (Exception e)
                {
                    return DataServiceResult.Failed($"Не удалось получить кадр из gif: {e.Message}");
                }
            }
        }
    }
}