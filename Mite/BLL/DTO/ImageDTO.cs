using Microsoft.AspNet.Identity;
using Mite.BLL.Core;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Hosting;

namespace Mite.BLL.DTO
{
    [Obsolete("Заменено ImagesHelper")]
    public class ImageDTO : IDisposable
    {
        private const string CompressedPostfix = "compressed";
        private Bitmap SourceBmp { get; set; }
        private string SaveVirtualFolder { get; set; }

        private string compressedFormat;
        private string CompressedFormat
        {
            get
            {
                if(compressedFormat == null)
                {
                    compressedFormat = VirtualPath.Split('.')[VirtualPath.Split('.').Length - 1];
                }
                return compressedFormat;
            }
            set
            {
                compressedFormat = value;
            }
        }

        public string VirtualPath { get; set; }
        private bool? compressedExists;
        /// <summary>
        /// Существует ли сжатый аналог изображения
        /// </summary>
        public bool CompressedExists
        {
            get
            {
                if(compressedExists == null)
                {
                    if (CompressedFormat == "png")
                    {
                        compressedExists = File.Exists(HostingEnvironment.MapPath(CompressedVirtualPath));
                        if (!(bool)compressedExists)
                        {
                            //В целях совместимости(раньше png сжималось в jpg, теперь может быть и png)
                            CompressedFormat = "jpg";
                            compressedExists = File.Exists(HostingEnvironment.MapPath(CompressedVirtualPath));
                        }
                    }
                    else if(CompressedFormat == "jpg" || CompressedFormat == "jpeg")
                    {
                        compressedExists = File.Exists(HostingEnvironment.MapPath(CompressedVirtualPath));
                    }
                    else
                    {
                        return false;
                    }
                }
                return compressedExists ?? false;
            }
        }
        public string CompressedVirtualPath
        {
            get
            {
                //Задаем путь для сохранения файла
                var path = SaveVirtualFolder;
                path += SaveVirtualFolder[SaveVirtualFolder.Length - 1] == '/' ? "" : "/";
                //Разделяем путь к файлу, чтобы получить его имя
                var pathArr = VirtualPath.Split('/');
                var fileName = pathArr[pathArr.Length - 1].Split('.')[0];
                    
                //Основываясь на названии самого изображения, создаем название сжатого изображения
                var fullName = $"{fileName}_{CompressedPostfix}.{CompressedFormat}";

                return path + fullName;
            }
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="virtualPath">Относительный путь к файлу</param>
        /// <param name="imagesFolder">Папка изображений</param>
        /// <param name="createBmp">Создавать ли Bitmap объект(обычно создается только при сжатии)</param>
        public ImageDTO(string virtualPath, string imagesFolder, bool createBmp = false)
        {
            VirtualPath = virtualPath;
            SaveVirtualFolder = imagesFolder;
            if (createBmp)
            {
                SourceBmp = new Bitmap(HostingEnvironment.MapPath(VirtualPath));
            }
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
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
        public DataServiceResult Compress()
        {
            if (SourceBmp.RawFormat.Guid != ImageFormat.Jpeg.Guid && SourceBmp.RawFormat.Guid != ImageFormat.Png.Guid)
            {
                return DataServiceResult.Failed("Изображение не подходит по формату");
            }
            if(SourceBmp.RawFormat.Guid == ImageFormat.Png.Guid)
            {
                var compressResult = CompressPng();
                if (!compressResult.Succeeded)
                {
                    CompressedFormat = "jpg";
                    return CompressJpg();
                }
            }
            else if(SourceBmp.RawFormat.Guid == ImageFormat.Jpeg.Guid)
            {
                return CompressJpg();
            }
            else if(SourceBmp.RawFormat.Guid == ImageFormat.Gif.Guid)
            {
                return CompressGif();
            }
            return DataServiceResult.Failed();
        }
        /// <summary>
        /// Сжимаем в Png
        /// </summary>
        /// <returns></returns>
        private DataServiceResult CompressPng()
        {
            return DataServiceResult.Success();
            //try
            //{
            //    var quantizer = new WuQuantizer();
            //    using(var quantized = quantizer.QuantizeImage(SourceBmp))
            //    {
            //        quantized.Save(HostingEnvironment.MapPath(CompressedVirtualPath));
            //    }
            //    return DataServiceResult.Success();
            //}
            //catch (Exception)
            //{
            //    return DataServiceResult.Failed();
            //}
        }
        /// <summary>
        /// Сжимаем в Jpeg
        /// </summary>
        /// <returns></returns>
        private DataServiceResult CompressJpg()
        {
            try
            {
                var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L);
                SourceBmp.Save(HostingEnvironment.MapPath(CompressedVirtualPath), jpgEncoder, encoderParams);
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
        private DataServiceResult CompressGif()
        {
            try
            {
                var frameDimension = new FrameDimension(SourceBmp.FrameDimensionsList[0]);
                SourceBmp.SelectActiveFrame(frameDimension, 0);
                SourceBmp.Save(HostingEnvironment.MapPath(CompressedVirtualPath));
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                return DataServiceResult.Failed($"Не удалось получить кадр из gif: {e.Message}");
            }
        }

        public void Dispose()
        {
            if(SourceBmp != null)
            {
                SourceBmp.Dispose();
            }
        }
    }
}