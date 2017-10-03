using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing.Imaging;
using System.Drawing;

namespace Mite.Tests.BLL
{
    //[TestClass]
    public class ImageDTOTest
    {
        [TestMethod]
        public void GifCompressTest()
        {
            using(var img = Image.FromFile(@"C:\Users\Buka\Desktop\4f109058-5a21-4fc7-8919-6712c699df15.gif"))
            {
                var frameDimension = new FrameDimension(img.FrameDimensionsList[0]);
                var frames = img.GetFrameCount(frameDimension);
                img.SelectActiveFrame(frameDimension, 0);
                img.Save(@"C:\Users\Buka\Desktop\4f109058-5a21-4fc7-8919-6712c699df15_compressed.jpg", ImageFormat.Jpeg);
            }
        }
    }
}
