using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Mite.Helpers
{
    public static class ImagesHelper
    {
        /// <summary>
        /// Сжимает и сохраняет изображение
        /// </summary>
        /// <param name="path">Путь к изображению</param>
        /// <param name="saveFolder">Папка для сохранения</param>
        public static void CompressImage(string path, string saveFolder)
        {
            var sourceBmp = new Bitmap(path);
            Bitmap jpegBmp;
            if(sourceBmp.RawFormat != ImageFormat.Jpeg && sourceBmp.RawFormat != ImageFormat.Png)
            {
                return;
            }
            if(sourceBmp.RawFormat == ImageFormat.Png)
            {
                var stream = new MemoryStream();

                var jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L);
                //jpegBmp.Save(@"C:\Users\Buka\Pictures\testwww.jpg", jpgEncoder, encoderParams);
                stream.Close();
                jpegBmp = new Bitmap(stream);
            }
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
    }
}