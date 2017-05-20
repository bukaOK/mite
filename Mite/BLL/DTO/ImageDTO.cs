using Microsoft.AspNet.Identity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Hosting;

namespace Mite.BLL.DTO
{
    public class ImageDTO
    {
        private const string compressedPostfix = "compressed";
        private string compressedVirtualPath;
        private string saveVirtualFolder;

        public string VirtualPath { get; set; }
        /// <summary>
        /// Существует ли сжатый аналог изображения
        /// </summary>
        public bool IsCompressedExists
        {
            get
            {
                return File.Exists(HostingEnvironment.MapPath(CompressedVirtualPath));
            }
        }
        public string CompressedVirtualPath
        {
            get
            {
                if(compressedVirtualPath == null)
                {
                    //Задаем путь для сохранения файла
                    var path = saveVirtualFolder;
                    path += saveVirtualFolder[saveVirtualFolder.Length - 1] == '/' ? "" : "/";
                    //Разделяем путь к файлу, чтобы получить его имя
                    var pathArr = VirtualPath.Split('/');
                    var fileName = pathArr[pathArr.Length - 1].Split('.')[0];
                    //Основываясь на названии самого изображения, создаем название сжатого изображения
                    var fullName = $"{fileName}_{compressedPostfix}.jpg";

                    compressedVirtualPath = path + fullName;
                }
                return compressedVirtualPath;
            }
        }
        public ImageDTO(string virtualPath, string imagesFolder)
        {
            VirtualPath = virtualPath;
            saveVirtualFolder = imagesFolder;
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
        public IdentityResult Compress()
        {
            var sourceBmp = new Bitmap(HostingEnvironment.MapPath(VirtualPath));
            //Поток понадобится, чтобы записать туда переформатированный png файл
            Bitmap jpgBmp;

            if (sourceBmp.RawFormat.Guid != ImageFormat.Jpeg.Guid && sourceBmp.RawFormat.Guid != ImageFormat.Png.Guid)
            {
                return IdentityResult.Failed("Изображение не подходит по формату");
            }
            jpgBmp = sourceBmp;
            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L);
            jpgBmp.Save(HostingEnvironment.MapPath(CompressedVirtualPath), jpgEncoder, encoderParams);
            jpgBmp.Dispose();
            return IdentityResult.Success;
        }
    }
}